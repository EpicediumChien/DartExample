using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
using DDPM.UI.Plugin.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using Windows.Gaming.Input;

namespace DDPM.UI.Plugin.ViewModels
{
    public class AirAudioViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables

        public readonly ILog _log;
        public IDeviceManagerSA _deviceManager;
        public IShowPluginManager _showPluginManager;
        public DeviceInfoDTP deviceInfoDTP;
        public string _current_AirAudio;
        private Debouncer _debouncerAirAudio;
        private Debouncer _debouncerAirAudioPauseMusic;
        private Debouncer _debouncerAirAudioMuteMicrophone;
        private Debouncer _debouncerAirAudioQuickPause;
        private Debouncer _debouncerAirAudioSidetoneCheck;
        public event EventHandler<EventArgs> AirAudioSettingChanged;
        public event EventHandler<EventArgs> AirAudioGroupChanged;
        public bool _waitAirAudioReady;
        #endregion Variables

        public new event PropertyChangedEventHandler? PropertyChanged;

        //
        public event EventHandler<string> AirAudioChanged;
        //

        public bool isAirAudio = false;
        public bool isAirAudioChange = false;
        public AirAudioViewModel(IShowPluginManager showPluginManager, IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;
            _showPluginManager = showPluginManager;
            deviceInfoDTP = new DeviceInfoDTP();
            _current_AirAudio = string.Empty;
            //DdpmCommonHelper.DeviceManagerSA!.UIUpdateNotify += AirAudio_DTPNotify;
            //DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;
            DebouncerFfunctionInit();
            _log!.Info($"[AirAudioViewModel] AirAudioViewModel Start...");
        }

        public void UloadAirAudio_DTPNotify()
        {
            AirAudioSettingChanged -= AirAudioSettingChanged;
            if (_deviceManager != null)
            {
                _deviceManager.UIUpdateNotify -= AirAudio_DTPNotify;
                DdpmCommonHelper.BitmapImageUpdated -= ImageUpdate;
            }
        }

        private void DebouncerFfunctionInit()
        {
            _debouncerAirAudio = new Debouncer(1000, ExecuteDebouncedAction);
            _debouncerAirAudioPauseMusic = new Debouncer(1000, ExecuteDebouncedActionForPauseMusic);
            _debouncerAirAudioMuteMicrophone = new Debouncer(1000, ExecuteDebouncedActionForMuteMicrophone);
            _debouncerAirAudioQuickPause = new Debouncer(1000, ExecuteDebouncedActionForQuickPause);
            _debouncerAirAudioSidetoneCheck = new Debouncer(1000, ExecuteDebouncedActionForSidetoneCheck);
        }

        private void ImageUpdate(OSThemeEnum oSThemeEnum)
        {
            if (oSThemeEnum == OSThemeEnum.Dark)
            {
                _isDarkTheme = true;
            }
            else
            {
                _isDarkTheme = false;
            }
            OnPropertyChanged(nameof(IsDarkTheme));
        }

        private Dictionary<string, string> deal_param(string param)
        {
            Dictionary<string, string> tmp = new Dictionary<string, string>();

            try
            {
                List<string> list = param.Split(new char[] { ';' }).ToList();
                foreach (string s in list)
                {
                    List<string> item = s.Split(new char[] { ':' }).ToList();
                    if (item.Count == 2)
                    {
                        tmp.Add(item[0], item[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Info($"[AirAudioViewModel] deal_param exception {ex.Message.ToString()}");
            }
            return tmp;
        }

        private void AirAudio_DTPNotify(object? sender, UpdateUINotify e)
        {
            Dictionary<string, string> event_param = deal_param(e.UI_Field_Name);
            try
            {
                if (event_param == null || event_param.Count == 0)
                {
                    _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify null or 0");
                    return;
                }
                if (!event_param.TryGetValue("Device", out var device))
                {
                    _log.Info($"[AirAudioViewModel] Device cannot be found in event_param");
                    return;
                }
                if (device == "AirAudio")
                {
                    if (!event_param.TryGetValue("EventType", out var eventtype))
                    {
                        _log.Info($"[AirAudioViewModel] EventType cannot be found in event_param");
                        return;
                    }
                    if (!event_param.TryGetValue("DeviceId", out var guid))
                    {
                        _log.Info($"[AirAudioViewModel] GUID cannot be found in event_param");
                        return;
                    }
                    _log.Info($"[AirAudioViewModel] EventType = {eventtype}");

                    switch (eventtype)
                    {
                        case "AirAudio_Connected":
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_Connected {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;
                        case "AirAudio_Disconnected":
                            _waitAirAudioReady = false;
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_Disconnected {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;
                        case "AirAudio_PairedHostNameChanged":
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_PairedHostNameChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;
                        case "AirAudio_WearDetectionChanged":
                            HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isWearDetectionStatus);
                            deviceInfoDTP.WearDetectionFromDTP = _isWearDetectionStatus;
                            CheckWearDetectionUI();
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_WearDetectionChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;

                        case "AirAudio_IsWearDetectionPauseMusicEnabledChanged":
                            HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isPauseMusicStatus);
                            deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = _isPauseMusicStatus;
                            CheckWearDetectionUI();
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_IsWearDetectionPauseMusicEnabledChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;

                        case "AirAudio_IsWearDetectionMuteMicEnabledChanged":
                            HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isMuteMicrophoneStatus);
                            deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = _isMuteMicrophoneStatus;
                            CheckWearDetectionUI();
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_IsWearDetectionMuteMicEnabledChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;

                        case "AirAudio_WearDetectionQuickPauseChanged":
                            HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isQuickPauseStatus);
                            deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = BoolToInt(_isQuickPauseStatus);
                            CheckWearDetectionUI();
                            break;

                        case "AirAudio_WearDetectionSensitivityChanged":
                            CheckWearDetectionUI();
                            break;
                        case "AirAudio_BandsGainChanged":
                            // DTH event still support, DTP keep empty.
                            break;
                        case "AirAudio_BoomMicChanged":
                            deviceInfoDTP.AnswerCall = event_param[eventtype].ToLower() == "true" ? true : false;
                            CheckAnswerCallUI(true);
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_BoomMicChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;
                        case "AirAudio_BoomMicSupportedChangedArgs":
                            deviceInfoDTP.IsAnswerCallSupported = event_param[eventtype].ToLower() == "true" ? true : false;
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_BoomMicSupportedChangedArgs {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;
                        case "AirAudio_FirmwareVersionChanged":
                            FirmwareVersion2 = event_param[eventtype];
                            FirmwareVersion2 = string.Join(".", FirmwareVersion2.ToCharArray());
                            FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";

                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify ... AirAudio_FirmwareVersionChanged ... {FirmwareVersion2} ...");
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_FirmwareVersionChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;
                        case "AirAudio_IsReadyChanged":
                            if (!_waitAirAudioReady)
                            {
                                _waitAirAudioReady = true;
                                //FirmwareVersion2 = _deviceManager.GetAirAudioFirmwareVersionAsync(CurrentDeviceID.ToString()).Result;
                                //FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";
                            }
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_IsReadyChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;
                        case "AirAudio_IsDirtyChanged":
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_IsDirtyChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;
                        case "AirAudio_SetFactoryResetAsyncValueForAirAudio":
                            if (CheckIsDirty())
                                return;
                            if (guid != CurrentDeviceInfo!.ID.ToString())
                                RestoreToDefault(false);
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_SetFactoryResetAsyncValueForAirAudio {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;
                        case "AirAudio_SetFactoryResetAsyncValueForAirAudioForCLI":
                            RestoreToDefault(false);// For CLI Update
                            _log.Info($"[AirAudioViewModel] AirAudio_DTPNotify AirAudio_SetFactoryResetAsyncValueForAirAudio {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;
                        default:
                            break;
                    }
                    UpdateResetToDefault();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[AirAudioViewModel] AirAudio_DTPNotify Exception = {ex.Message.ToString()}");
            }
        }

        private void HandleWearDetectionEvent(string eventtype, string paramValue, ref bool statusField)
        {
            try
            {
                if (StringToBool(paramValue, out bool result))
                {
                    statusField = result;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[AirAudioViewModel] HandleWearDetectionEvent Exception = {ex.Message.ToString()}");
            }
        }

        public static bool StringToBool(string input, out bool result)
        {
            if (bool.TryParse(input, out result))
            {
                return true;
            }

            switch (input.ToLower())
            {
                case "normal":
                case "yes":
                case "1":
                    result = true;
                    return true;
                case "off":
                case "low":
                case "no":
                case "0":
                    result = false;
                    return true;
                default:
                    result = false;
                    return false;
            }

        }

        private void ExecuteDebouncedAction(object param)
        {
            _isRestoreEnable = false;
            if (param is string mode)
            {
                switch (mode)
                {
                    // Button類
                    case "ANC":
                    case "Transparency":
                    case "NoiseOff":

                        _log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetAirAudioAncModeAsync ... {mode} ... {deviceInfoDTP.AncMode.ToString()}");
                        _deviceManager.SetAirAudioAncModeAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.AncMode).Wait();
                        _log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetAirAudioSidetoneAsync ... SidetoneCheck .... {deviceInfoDTP.Sidetone.ToString()}");
                        _deviceManager.SetAirAudioSidetoneAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.Sidetone).Wait();

                        break;
                    case "TransparencylevelSlider":

                        _log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetAirAudioAncGainAsync ... Transparencylevel ... {deviceInfoDTP.AncGain.ToString()}");
                        _deviceManager.SetAirAudioAncGainAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.AncGain).Wait();

                        break;
                    //------------------------------------------------------------------------------------------
                    // Button類
                    case "DefaultCheck":
                    case "BassBoostCheck":
                    case "SpeechBoostCheck":
                    case "TrebleBoostCheck":
                    case "CustomCheck":

                        _log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetAirAudioSelectedPresetAsync ... {mode} ... {deviceInfoDTP.SelectedPreset.ToString()}");
                        _deviceManager.SetAirAudioSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.SelectedPreset).Wait();

                        break;
                    //------------------------------------------------------------------------------------------
                    case "WearDetectionCheck":

                        _log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetAirAudioWearDetectionAsync ... WearDetectionCheck ... {deviceInfoDTP.WearDetectionFromDTP.ToString()}");
                        _deviceManager.SetAirAudioWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.WearDetectionFromDTP).Wait();

                        break;
                    case "PauseMusicCheck":
                    //_log.Info($"[AirAudioViewModel] SetIsWearDetectionPauseMusicEnabledAsync ... PauseMusicCheck ... {deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()} ...");
                    //_deviceManager.SetIsWearDetectionPauseMusicEnabledAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP).Wait();
                    //break;
                    case "MuteMicrophoneCheck":
                    //_log.Info($"[AirAudioViewModel] SetIsWearDetectionMuteMicEnabledAsync ... MuteMicrophoneCheck ... {deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP.ToString()}");
                    //_deviceManager.SetIsWearDetectionMuteMicEnabledAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP).Wait();
                    //break;
                    // Button類
                    case "QuickPauseCheck":
                    case "NormalCheck":
                    case "SensitiveCheck":

                        _log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetAirAudioWearDetectionQuickPauseAsync ... {mode} ... {deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP.ToString()}");
                        _deviceManager.SetAirAudioWearDetectionQuickPauseAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP).Wait();

                        break;
                    //------------------------------------------------------------------------------------------
                    // Button類
                    case "Normal2Check":
                    case "LowCheck":

                        _log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetAirAudioWearDetectionSensitivityAsync ... {mode} ... {deviceInfoDTP.WearDetectionSensitivityFromDTP.ToString()}");
                        _deviceManager.SetAirAudioWearDetectionSensitivityAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.WearDetectionSensitivityFromDTP).Wait();

                        break;
                    //------------------------------------------------------------------------------------------
                    case "AnswerCallsCheck":
                    //_log.Info($"[AirAudioViewModel] SetBoomMicAsync ... AnswerCallsCheck .... {deviceInfoDTP.AnswerCall.ToString()}");
                    //_deviceManager.SetBoomMicAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.AnswerCall).Wait();
                    //break;
                    //------------------------------------------------------------------------------------------
                    case "BusyLightCheck":
                        //_log.Info($"[AirAudioViewModel] SetBusyLightAsync ....... {deviceInfoDTP.BusyLight.ToString()}");
                        //_deviceManager.SetBusyLightAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.BusyLight).Wait();
                        break;
                    //------------------------------------------------------------------------------------------
                    // Button類
                    case "EssentialCheck":
                    case "AllCheck":

                        _log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetAirAudioVoiceGuidanceAsync ... {mode} .... {deviceInfoDTP.BusyLight.ToString()}");
                        _deviceManager.SetAirAudioVoiceGuidanceAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.VoiceGuidance).Wait();

                        break;
                    //------------------------------------------------------------------------------------------
                    case "OutgoingAudioCheck":
                    //_log.Info($"[AirAudioViewModel] SetMicNoiseCancellationAsync ... OutgoingAudioCheck .... {deviceInfoDTP.MicNoiseCancellation.ToString()}");
                    //_deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.MicNoiseCancellation).Wait();
                    //break;
                    case "IncomingAudioCheck":
                    //_log.Info($"[AirAudioViewModel] SetMicNCIncomingAsync ... IncomingAudioCheck .... {deviceInfoDTP.MicNCIncoming.ToString()}");
                    //_deviceManager.SetMicNCIncomingAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.MicNCIncoming).Wait();
                    //break;
                    case "MicNoiseCancellationCheck":
                    //_log.Info($"[AirAudioViewModel] SetMicNoiseCancellationAsync ... MicNoiseCancellationCheck .... {deviceInfoDTP.MicNoiseCancellation.ToString()}");
                    //_deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.MicNoiseCancellation).Wait();
                    //break;
                    case "SidetoneCheck":
                        //_log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetSidetoneAsync ... SidetoneCheck .... {deviceInfoDTP.Sidetone.ToString()}");
                        //_deviceManager.SetSidetoneAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.Sidetone).Wait();
                        break;
                    case "SidetoneSlider":

                        _log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetAirAudioSidetoneLevelAsync ... SidetoneLevel .... {deviceInfoDTP.SidetoneLevel.ToString()}");
                        _deviceManager.SetAirAudioSidetoneLevelAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.SidetoneLevel).Wait();

                        break;
                    //------------------------------------------------------------------------------------------
                    default:
                        break;
                }
            }
        }

        private void ExecuteDebouncedActionForPauseMusic(object param)
        {
            _isRestoreEnable = false;

            _log.Info($"[AirAudioViewModel] ExecuteDebounced SetAirAudioIsWearDetectionPauseMusicEnabledAsync ... PauseMusicCheck ... {deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()} ...");
            _deviceManager.SetAirAudioIsWearDetectionPauseMusicEnabledAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP).Wait();

        }

        private void ExecuteDebouncedActionForMuteMicrophone(object param)
        {
            _isRestoreEnable = false;

            _log.Info($"[AirAudioViewModel] ExecuteDebounced SetAirAudioIsWearDetectionMuteMicEnabledAsync ... MuteMicrophoneCheck ... {deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP.ToString()}");
            _deviceManager.SetAirAudioIsWearDetectionMuteMicEnabledAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP).Wait();

        }
        private void ExecuteDebouncedActionForQuickPause(object param)
        {
            _isRestoreEnable = false;

            _log.Info($"[AirAudioViewModel] ExecuteDebounced SetAirAudioWearDetectionQuickPauseAsync ... QuickPauseCheck ... {deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP.ToString()}");
            _deviceManager.SetAirAudioWearDetectionQuickPauseAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP).Wait();

        }
        private void ExecuteDebouncedActionForSidetoneCheck(object param)
        {
            _isRestoreEnable = false;

            _log.Info($"[AirAudioViewModel] ExecuteDebouncedAction SetAirAudioSidetoneAsync ... SidetoneCheck .... {deviceInfoDTP.Sidetone.ToString()}");
            _deviceManager.SetAirAudioSidetoneAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.Sidetone).Wait();

        }

        private bool _isDarkTheme;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (_isDarkTheme != value)
                {
                    _isDarkTheme = value;
                    OnPropertyChanged(nameof(IsDarkTheme));
                }
            }
        }

        public void DetectPageShow(string model)
        {
            _log!.Info($"[AirAudioViewModel] DetectPageShow ... {model}");
            ReadQRCodeReg();
            AllResetAirAudioPage();
            CheckAirAudioFunc();
        }
        public void ReadQRCodeReg()
        {
            object regValue = null;
            try
            {
                if (_deviceManager != null)
                {
                    regValue = _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, RegPath, RegKeyForQRCode).Result;

                    if (regValue != null)
                    {

                        if (Convert.ToBoolean(regValue))
                        {
                            _deviceSettingsDownloadDellAudioPageShow = false;
                            _log.Info($"[AirAudioViewModel] ReadQRCodeReg ....... success true");
                        }
                        else
                        {
                            _deviceSettingsDownloadDellAudioPageShow = true;
                            _log.Info($"[AirAudioViewModel] ReadQRCodeReg ....... success false");
                        }

                    }
                    else
                    {
                        _deviceSettingsDownloadDellAudioPageShow = true;
                        _log.Info($"[AirAudioViewModel] ReadQRCodeReg ReadRegistryData ....... fail");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Info($"[AirAudioViewModel] ReadQRCodeReg ....... {ex.ToString()}");
            }
        }


        public void AllResetAirAudioPage()
        {
            _log.Info($"[AirAudioViewModel] AllResetAirAudioPage ...");
            _controlTheNoiseIHearPageShow = false;
            _configureMyAudioModesPageShow = false;
            _wearDetectionPageShow = false;
            _automatedActionsWhenAirAudioIsRemovedPageShow = false;
            _automatedActionsQuickPausePageShow = false;
            _automatedActionsSensitivityPageShow = false;
            _voiceGuidancePageShow = false;
            //_deviceSettingsDownloadDellAudioPageShow = false;
            _automatedActionsSensitivityUpPageShow = false;
            _automatedActionsAnswerCallPageShow = false;
        }

        public void CheckAirAudioFunc()
        {
            _log.Info($"[AirAudioViewModel] CheckAirAudioFunc ...");
            CheckSidetoneUI(true);
            CheckBusyLightUI(true);
            CheckOutgoingAudioUI(true);
            CheckMicNCIncomingUI(true);
            CheckMicNoiseCancellationUI(true);
            CheckWearDetectionUI(true);
            CheckPresetsUI(true);
            CheckVoiceGuidanceUI(true);
            CheckANCUI(true);
            CheckAnswerCallUI(true);
            AirAudioSettingChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CheckSidetoneUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.IsSidetoneSupported)
            {
                _isSidetoneStatus = deviceInfoDTP.Sidetone;//_deviceManager.GetSidetoneAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo.Sidetone;

                if (PropertyChange)
                {
                    OnPropertyChanged(nameof(SidetoneStatus));
                    OnPropertyChanged(nameof(Sidetone_String));
                    OnPropertyChanged(nameof(SidetoneSliderStatus));
                    UpdateCollaborationAndultimediaUI(true, false);
                }
            }
        }

        private void CheckSidetoneLevelUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.IsSidetoneSupported)
            {
                int sidevalue = deviceInfoDTP.SidetoneLevel;//_deviceManager.GetSidetoneLevelAsync(CurrentDeviceInfo!.ID.ToString()).Result;
                if (_isidetoneSliderValue != sidevalue)//CurrentDeviceInfo.SidetoneLevel)
                {
                    _isidetoneSliderValue = sidevalue;

                    if (PropertyChange)
                    {
                        OnPropertyChanged(nameof(SidetoneSliderValue));
                        OnPropertyChanged(nameof(SidetoneSliderStatus));
                        UpdateCollaborationAndultimediaUI(true, false);
                    }
                }
            }
        }

        private void CheckAnswerCallUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.IsAnswerCallSupported)
                _isAnswerCallsStatus = deviceInfoDTP.AnswerCall;
            else
                _isAnswerCallsStatus = false;
            if (PropertyChange)
            {
                OnPropertyChanged(nameof(AnswerCallsStatus));
                OnPropertyChanged(nameof(AnswerCalls_String));
            }
        }

        private void CheckBusyLightUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.IsBusyLightSupported)
                _isBusyLightStatus = deviceInfoDTP.BusyLight;//_deviceManager.GetBusyLightAsync(CurrentDeviceInfo!.ID.ToString()).Result; //CurrentDeviceInfo.BusyLight;
            if (PropertyChange)
            {
                OnPropertyChanged(nameof(BusyLightStatus));
                OnPropertyChanged(nameof(BusyLight_String));
            }
        }
        private void CheckOutgoingAudioUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.IsMicNoiseCancellationSupported &&
                _isOutgoingAudioStatus != deviceInfoDTP.MicNoiseCancellation)
            {
                _isOutgoingAudioStatus = deviceInfoDTP.MicNoiseCancellation;

                if (PropertyChange)
                {
                    OnPropertyChanged(nameof(OutgoingAudioStatus));
                    OnPropertyChanged(nameof(OutgoingAudio_String));
                    UpdateCollaborationAndultimediaUI(true, false);
                }
            }
        }

        private void CheckMicNCIncomingUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.IsMicNCIncomingSupported &&
                _isIncomingAudioStatus != deviceInfoDTP.MicNCIncoming)
            {
                _isIncomingAudioStatus = deviceInfoDTP.MicNCIncoming;

                if (PropertyChange)
                {
                    OnPropertyChanged(nameof(IncomingAudioStatus));
                    OnPropertyChanged(nameof(IncomingAudio_String));
                    UpdateCollaborationAndultimediaUI(true, false);
                }
            }
        }

        private void CheckMicNoiseCancellationUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.IsMicNoiseCancellationSupported)
                _isMicNoiseCancellationStatus = deviceInfoDTP.MicNoiseCancellation;

            if (PropertyChange)
            {
                OnPropertyChanged(nameof(MicNoiseCancellationStatus));
                OnPropertyChanged(nameof(MicNoiseCancellation_String));
                UpdateCollaborationAndultimediaUI(true, false);
            }
        }

        private void CheckWearDetectionUI()
        {
            OnPropertyChanged(nameof(WearDetectionStatus));
            OnPropertyChanged(nameof(WearDetection_String));

            OnPropertyChanged(nameof(PauseMusicStatus));
            OnPropertyChanged(nameof(PauseMusic_String));

            OnPropertyChanged(nameof(MuteMicrophoneStatus));
            OnPropertyChanged(nameof(MuteMicrophone_String));

            OnPropertyChanged(nameof(IsNormal2Checked));
            OnPropertyChanged(nameof(IsLowChecked));

            OnPropertyChanged(nameof(QuickPauseStatus));
            OnPropertyChanged(nameof(QuickPause_String));

            OnPropertyChanged(nameof(IsNormalChecked));
            OnPropertyChanged(nameof(IsSensitiveChecked));
        }

        private void CheckWearDetectionUIForDTH(bool PropertyChange)
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

        private void CheckWearDetectionUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.IsWearDetectionSupported)
            {
                _isWearDetectionStatus = deviceInfoDTP.WearDetectionFromDTP;
                _isPauseMusicStatus = deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP;
                _isMuteMicrophoneStatus = deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP;
                _isQuickPauseStatus = IntToBool(deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP);

                if (Model == "WL7024")
                {
                    if (deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP == 0)
                    {
                        _isQuickPauseStatus = false;
                        _isNormalChecked = true;
                        _isSensitiveChecked = false;
                    }
                    if (deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP == 2)
                    {
                        _isQuickPauseStatus = true;
                        _isNormalChecked = false;
                        _isSensitiveChecked = true;
                    }
                    if (deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP == 1)
                    {
                        if (!_isQuickPauseStatus)
                        {
                            _isQuickPauseStatus = !_isQuickPauseStatus;
                        }
                        _isNormalChecked = true;
                        _isSensitiveChecked = false;
                    }
                }

                if (IntToBool(deviceInfoDTP.WearDetectionSensitivityFromDTP))
                {
                    if (Model == "WL5024")
                    {
                        _isLowChecked = false;
                        _isNormal2Checked = true;
                    }
                }
                else
                {
                    if (Model == "WL5024")
                    {
                        _isLowChecked = true;
                        _isNormal2Checked = false;
                    }
                }

                if (PropertyChange)
                {
                    OnPropertyChanged(nameof(WearDetectionStatus));
                    OnPropertyChanged(nameof(WearDetection_String));

                    OnPropertyChanged(nameof(PauseMusicStatus));
                    OnPropertyChanged(nameof(PauseMusic_String));

                    OnPropertyChanged(nameof(MuteMicrophoneStatus));
                    OnPropertyChanged(nameof(MuteMicrophone_String));

                    OnPropertyChanged(nameof(IsNormal2Checked));
                    OnPropertyChanged(nameof(IsLowChecked));

                    OnPropertyChanged(nameof(QuickPauseStatus));
                    OnPropertyChanged(nameof(QuickPause_String));

                    OnPropertyChanged(nameof(IsNormalChecked));
                    OnPropertyChanged(nameof(IsSensitiveChecked));
                }
            }
        }

        private void CheckPresetsUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.IsPresetsSupported)
            {
                switch (deviceInfoDTP.SelectedPreset)
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
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = true;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        break;

                    case 3:
                        _isDefaultChecked = false;
                        _isBassBoostChecked = true;
                        _isSpeechBoostChecked = false;
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
                if (isAirAudio)
                {
                    isAirAudioChange = SetDefualAirAudioChange();
                }
                if (PropertyChange)
                {
                    UpdateCollaborationAndultimediaUI(false, true);
                    OnPropertyChanged(nameof(IsCollaborationChecked));
                    OnPropertyChanged(nameof(IsMultimediaChecked));
                    OnPropertyChanged(nameof(IsDefaultChecked));
                    OnPropertyChanged(nameof(IsBassBoostChecked));
                    OnPropertyChanged(nameof(IsSpeechBoostChecked));
                    OnPropertyChanged(nameof(IsTrebleBoostChecked));
                    OnPropertyChanged(nameof(IsCustomChecked));
                }
            }
        }

        private void CheckVoiceGuidanceUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.VoiceGuidance)
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
                OnPropertyChanged(nameof(IsAllChecked));
                OnPropertyChanged(nameof(IsEssentialChecked));
            }
        }

        private void CheckANCUI(bool PropertyChange)
        {
            if (deviceInfoDTP!.IsANCSupported)
            {
                switch (deviceInfoDTP.AncMode)
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
                        _isTransparencylevelSliderValue = deviceInfoDTP.AncGain;
                        break;
                }
            }
            else
            {
                _isNoiseOffChecked = false;
                _isActiveNoiseCancellingChecked = false;
                _isTransparencyChecked = false;
            }
            OnPropertyChanged(nameof(IsNoiseOffChecked));
            OnPropertyChanged(nameof(IsActiveNoiseCancellingChecked));
            OnPropertyChanged(nameof(IsTransparencyChecked));
            OnPropertyChanged(nameof(TransparencylevelSliderValue));
        }

        private void UpdateCollaborationAndultimediaUI(bool Collaboration, bool Multimedia)
        {
            _isCollaborationChecked = Collaboration;
            _isMultimediaChecked = Multimedia;
            OnPropertyChanged(nameof(IsCollaborationChecked));
            OnPropertyChanged(nameof(IsMultimediaChecked));
        }

        public void UpdateResetToDefault()
        {
            //if (isAirAudio)
            //{
            //    if (isAirAudioChange)
            //    {
            //        _isRestoreEnable = true;
            //    }
            //    else
            //    {
            //        _isRestoreEnable = false;
            //    }
            //    return;
            //}
            if (CheckIsDirty())
                IsRestoreEnable = true;
            else
                IsRestoreEnable = false;
            OnPropertyChanged(nameof(IsRestoreEnable));
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
                if (deviceInfo.LogicalDeviceType.Contains("AirAudio"))
                    DeviceInfos.Add(deviceInfo.ID, deviceInfo);
            }
        }

        public override bool SetCurrentDevice(string instanceIDs)
        {
            _log.Info($"[AirAudioViewModel] SetCurrentDevice ... instanceIDs : {instanceIDs}");
            if (!base.SetCurrentDevice(instanceIDs))
                return false;
            _waitAirAudioReady = false; // Before enter, make sure to reset the flag
            deviceInfoDTP = new DeviceInfoDTP();
            _deviceManager.UIUpdateNotify += AirAudio_DTPNotify;
            DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;
            isAirAudio = true;
            isAirAudioChange = SetDefualAirAudioChange();
            _log.Info($"[AirAudioViewModel] SetCurrentDevice ... SB725");
            _log.Info($"[AirAudioViewModel] SetCurrentDevice GUID ... {CurrentDeviceID.ToString()}");
            return true;
        }

        private bool SetDefualAirAudioChange()
        {
            if (IsDefaultChecked)
            {
                return false;
            }
            if (IsSpeechBoostChecked)
            {
                return true;
            }
            if (IsTrebleBoostChecked)
            {
                return true;
            }
            if (IsBassBoostChecked)
            {
                return true;
            }
            if (IsCustomChecked)
            {
                return true;
            }


            return false;
        }

        public override void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            _log.Info($"[AirAudioViewModel] HandleNotification ... Receive {property.ToString()}");
            base.HandleNotification(changeType, di, property);
            //CheckAirAudioFunc();
            switch (property)
            {
                case "IsReadyChanged":
                    if (!_waitAirAudioReady)
                    {
                        _waitAirAudioReady = true;
                        //FirmwareVersion2 = _deviceManager.GetAirAudioFirmwareVersionAsync(CurrentDeviceID.ToString()).Result;
                        //FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";
                    }
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event IsReadyChanged {Model.ToString() + " : " + di.IsReady.ToString()}");
                    break;
                case "IsDirtyChanged":
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event IsDirtyChanged {Model.ToString() + " : " + di.IsDirty.ToString()}");
                    break;
                case "MicNoiseCancellationChanged":
                    deviceInfoDTP.MicNoiseCancellation = di.MicNoiseCancellation;//_deviceManager.GetMicNoiseCancellationAsync(CurrentDeviceID.ToString()).Result;
                    CheckMicNoiseCancellationUI(true);
                    CheckOutgoingAudioUI(true);
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event MicNoiseCancellationChanged {Model.ToString() + " : " + di.MicNoiseCancellation.ToString()}");
                    break;

                case "MicNCIncomingChanged":
                    deviceInfoDTP.MicNCIncoming = di.MicNCIncoming;//_deviceManager.GetMicNCIncomingAsync(CurrentDeviceID.ToString()).Result;
                    CheckMicNCIncomingUI(true);
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event MicNCIncomingChanged {Model.ToString() + " : " + di.MicNCIncoming.ToString()}");
                    break;

                case "SidetoneChanged":
                    deviceInfoDTP.Sidetone = di.Sidetone;//_deviceManager.GetSidetoneAsync(CurrentDeviceID.ToString()).Result;
                    CheckSidetoneUI(true);
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event SidetoneChanged {Model.ToString() + " : " + di.Sidetone.ToString()}");
                    break;

                case "BusyLightChanged":
                    deviceInfoDTP.BusyLight = di.BusyLight;//_deviceManager.GetBusyLightAsync(CurrentDeviceID.ToString()).Result;
                    CheckBusyLightUI(true);
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event BusyLightChanged {Model.ToString() + " : " + di.BusyLight.ToString()}");
                    break;

                case "VoiceGuidanceChanged":
                    deviceInfoDTP.VoiceGuidance = di.VoiceGuidance;//_deviceManager.GetVoiceGuidanceAsync(CurrentDeviceID.ToString()).Result;
                    CheckVoiceGuidanceUI(true);
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event VoiceGuidanceChanged {Model.ToString() + " : " + di.VoiceGuidance.ToString()}");
                    break;

                case "SelectedPresetChanged":
                    deviceInfoDTP.SelectedPreset = di.SelectedPreset;//_deviceManager.GetSelectedPresetAsync(CurrentDeviceID.ToString()).Result;
                    CheckPresetsUI(true);
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event SelectedPresetChanged {Model.ToString() + " : " + di.SelectedPreset.ToString()}");
                    break;

                case "SidetoneLevelChanged":
                    deviceInfoDTP.SidetoneLevel = di.SidetoneLevel;//_deviceManager.GetSidetoneLevelAsync(CurrentDeviceID.ToString()).Result;
                    CheckSidetoneLevelUI(true);
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event SidetoneLevelChanged {Model.ToString() + " : " + di.SidetoneLevel.ToString()}");
                    break;
                //case "MuteStatusChanged":
                //    break;
                case "BandsGainChanged":
                    deviceInfoDTP.Band1Gain = di.Band1Gain;
                    deviceInfoDTP.Band2Gain = di.Band2Gain;
                    deviceInfoDTP.Band3Gain = di.Band3Gain;
                    deviceInfoDTP.Band4Gain = di.Band4Gain;
                    deviceInfoDTP.Band5Gain = di.Band5Gain;
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event BandsGainChanged {Model.ToString() + " : " + "Band1Gain = " + di.Band1Gain.ToString()}"
                                                                                                                           + ", Band2Gain = " + di.Band2Gain.ToString()
                                                                                                                           + ", Band3Gain = " + di.Band3Gain.ToString()
                                                                                                                           + ", Band4Gain = " + di.Band4Gain.ToString()
                                                                                                                           + ", Band5Gain = " + di.Band5Gain.ToString());
                    AirAudioSettingChanged?.Invoke(this, EventArgs.Empty);
                    break;

                case "AncModeChanged":
                    deviceInfoDTP.AncMode = di.AncMode;//_deviceManager.GetAncModeAsync(CurrentDeviceID.ToString()).Result;
                    CheckANCUI(true);
                    // PIMS-333300
                    switch (deviceInfoDTP.AncMode)
                    {
                        case 0:
                        case 1:
                            deviceInfoDTP.Sidetone = true;
                            _isSidetoneStatus = true;
                            break;

                        case 2:
                            deviceInfoDTP.Sidetone = false;
                            _isSidetoneStatus = false;
                            break;
                    }
                    if (IsDTPReady)
                    {
                        _deviceManager.SetSidetoneAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.Sidetone).Wait();
                    }
                    else
                        _deviceManager.SetSidetone(true, CurrentDeviceInfo!.ID).Wait();
                    CheckSidetoneUI(true);
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event AncModeChanged {Model.ToString() + " : " + di.AncMode.ToString()}");
                    break;

                case "AncGainChanged":
                    deviceInfoDTP.AncGain = di.AncGain;
                    _isTransparencylevelSliderValue = di.AncGain;
                    OnPropertyChanged(nameof(TransparencylevelSliderValue));
                    _log.Info($"[AirAudioViewModel] HandleNotification DTH Event AncGainChanged {Model.ToString() + " : " + di.AncGain.ToString()}");
                    break;
                default:
                    break;
            }
            UpdateResetToDefault();
            switch (changeType)
            {
                case DeviceChangedType.Peripherals_SettingsChange:
                    if (DeviceInfos.ContainsKey(di.ID))
                    {
                        DeviceInfos.Remove(di.ID);
                        DeviceInfos.Add(di.ID, di);
                        _log.Info($"[AirAudioViewModel] HandleNotification DTH Event Peripherals_SettingsChange {Model.ToString() + " : " + di.ID.ToString()}");
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
        public async void RestoreToDefault(bool set = true)
        {
            try
            {
                if (IsDTPReady)
                {
                    _log.Info($"[AirAudioViewModel] Print before property ...RestoreToDefault ... in");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.Band1Gain ...........= {deviceInfoDTP.Band1Gain.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.Band2Gain ...........= {deviceInfoDTP.Band2Gain.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.Band3Gain ...........= {deviceInfoDTP.Band3Gain.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.Band4Gain ...........= {deviceInfoDTP.Band4Gain.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.Band5Gain ...........= {deviceInfoDTP.Band5Gain.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.AncGain .............= {deviceInfoDTP.AncGain.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.AncMode .............= {deviceInfoDTP.AncMode.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.BatteryLevel ........= {deviceInfoDTP.BatteryLevel.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.BusyLight ...........= {deviceInfoDTP.BusyLight.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.MicNoiseCancellation = {deviceInfoDTP.MicNoiseCancellation.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.MicNCIncoming .......= {deviceInfoDTP.MicNCIncoming.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.Sidetone ............= {deviceInfoDTP.Sidetone.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.SidetoneLevel .......= {deviceInfoDTP.SidetoneLevel.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.SelectedPreset ......= {deviceInfoDTP.SelectedPreset.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.VoiceGuidance .......= {deviceInfoDTP.VoiceGuidance.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.WearDetection .......= {deviceInfoDTP.WearDetectionFromDTP.ToString()}");
                    _log.Info($"[AirAudioViewModel] deviceInfoDTP.AnswerCall ..........= {deviceInfoDTP.AnswerCall.ToString()}");
                    if (set)
                    {
                        //if (isAirAudio == false)
                        {
                            bool SetFactoryResult = _deviceManager.SetFactoryResetAsyncValueForAirAudioAsync(CurrentDeviceInfo!.ID.ToString(), true).Result;
                            _log.Info($"[AirAudioViewModel] SetFactoryResetAsyncValueForAirAudio = {SetFactoryResult.ToString()}");
                        }
                    }
                    else
                        _log.Info($"[AirAudioViewModel] SetFactoryResetAsyncValueForAirAudio only Update UI");
                    UpdateDTPValue();
                }
                //else
                //{
                //    _log.Info($"[AirAudioViewModel] DTH Print before property ...RestoreToDefault ... in");
                //    SetFactoryResetForDTH();
                //    UpdateDTHValue();
                //}
                if (isAirAudio)
                {
                    AirAudioChanged?.Invoke(this, "RestoreToDefault");
                    return;
                }
                CheckAirAudioFunc();
                UpdateResetToDefault();
                //AirAudioSettingChanged?.Invoke(this, EventArgs.Empty); //CheckAirAudioFunc();裡已經有執行
                //_showPluginManager?.ShowHomePage();
            }
            catch (Exception ex)
            {
                _log!.Error($"[AirAudioViewModel] RestoreToDefault ...... {ex.ToString()}");
            }
        }

        //private void SetFactoryResetForDTH()
        //{
        //    try
        //    {
        //        _log.Info($"[AirAudioViewModel] DTH SetFactoryResetForDTH ......");
        //        _log.Info($"[AirAudioViewModel] DTH ***********************************************************************");

        //        if (CurrentDeviceInfo == null)
        //        {
        //            _log.Error($"[AirAudioViewModel] CurrentDeviceInfo is null, unable to reset settings.");
        //            return;
        //        }

        //        string currentModel = CurrentDeviceInfo.ModelNumber;

        //        if (!ModelDefaultSettings.ContainsKey(currentModel))
        //        {
        //            _log.Warning($"[AirAudioViewModel] No default settings found for model: {currentModel}");
        //            return;
        //        }

        //        _log.Info($"[AirAudioViewModel] DTH SetFactoryResetForDTH currentModel = {currentModel}");

        //        var defaultSettings = ModelDefaultSettings[currentModel];

        //        deviceInfoDTP.AncMode = defaultSettings.AncMode;
        //        deviceInfoDTP.AncGain = defaultSettings.AncGain;
        //        deviceInfoDTP.BusyLight = defaultSettings.BusyLight;
        //        deviceInfoDTP.MicNoiseCancellation = defaultSettings.MicNoiseCancellation;
        //        deviceInfoDTP.Sidetone = defaultSettings.Sidetone;
        //        deviceInfoDTP.VoiceGuidance = defaultSettings.VoiceGuidance;
        //        deviceInfoDTP.SelectedPreset = defaultSettings.SelectedPreset;
        //        deviceInfoDTP.Band1Gain = defaultSettings.Band1Gain;
        //        deviceInfoDTP.Band2Gain = defaultSettings.Band2Gain;
        //        deviceInfoDTP.Band3Gain = defaultSettings.Band3Gain;
        //        deviceInfoDTP.Band4Gain = defaultSettings.Band4Gain;
        //        deviceInfoDTP.Band5Gain = defaultSettings.Band5Gain;
        //        deviceInfoDTP.MicNCIncoming = defaultSettings.MicNCIncoming;
        //        deviceInfoDTP.WearDetectionFromDTP = defaultSettings.WearDetectionFromDTP;
        //        deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = defaultSettings.IsWearDetectionPauseMusicEnableFromDTP;
        //        deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = defaultSettings.IsWearDetectionMuteMicEnabledFromDTP;
        //        deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = defaultSettings.WearDetectionQuickPauseAsyncFromDTP;
        //        deviceInfoDTP.WearDetectionSensitivityFromDTP = defaultSettings.WearDetectionSensitivityFromDTP;
        //        deviceInfoDTP.AnswerCall = defaultSettings.AnswerCall;

        //        //DTH no WearDetection func
        //        //Read this AirAudio Support function
        //        if (CurrentDeviceInfo!.IsANCSupported)
        //        {
        //            deviceInfoDTP.IsANCSupported = true;
        //            _deviceManager.SetAncMode(deviceInfoDTP.AncMode, CurrentDeviceInfo!.ID).Wait();
        //            _deviceManager.SetAncGain(deviceInfoDTP.AncGain, CurrentDeviceInfo!.ID).Wait();
        //            _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.AncMode .....................= {deviceInfoDTP.AncMode.ToString()}");
        //            _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.AncGain .....................= {deviceInfoDTP.AncGain.ToString()}");
        //        }
        //        else
        //        {
        //            deviceInfoDTP.IsANCSupported = false;
        //            _log.Info($"[AirAudioViewModel] DTH GetIsANCSupportedAsync .............................. NO");
        //        }

        //        //------------------------------------------------------------------------------------
        //        if (CurrentDeviceInfo!.IsBusyLightSupported)
        //        {
        //            deviceInfoDTP.IsBusyLightSupported = true;
        //            _deviceManager.SetBusyLight(deviceInfoDTP.BusyLight, CurrentDeviceInfo!.ID).Wait();
        //            _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.BusyLight ...................= {deviceInfoDTP.BusyLight.ToString()}");
        //        }
        //        else
        //        {
        //            deviceInfoDTP.IsBusyLightSupported = false;
        //            _log.Info($"[AirAudioViewModel] DTH GetIsBusyLightSupportedAsync ........................ NO");
        //        }
        //        //------------------------------------------------------------------------------------

        //        if (CurrentDeviceInfo!.IsMicNoiseCancellationSupported)
        //        {
        //            deviceInfoDTP.IsMicNoiseCancellationSupported = true;
        //            _deviceManager.SetMicNoiseCancellation(deviceInfoDTP.MicNoiseCancellation, CurrentDeviceInfo!.ID).Wait();
        //            _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.MicNoiseCancellation .............= {deviceInfoDTP.MicNoiseCancellation.ToString()}");
        //        }
        //        else
        //        {
        //            deviceInfoDTP.IsMicNoiseCancellationSupported = false;
        //            _log.Info($"[AirAudioViewModel] DTH GetIsMicNoiseCancellationSupportedAsync ............. NO");
        //        }
        //        //------------------------------------------------------------------------------------

        //        if (CurrentDeviceInfo!.IsSidetoneSupported)
        //        {
        //            deviceInfoDTP.IsSidetoneSupported = true;
        //            _deviceManager.SetSidetone(deviceInfoDTP.Sidetone, CurrentDeviceInfo!.ID).Wait();
        //            _deviceManager.SetSidetoneLevel(deviceInfoDTP.SidetoneLevel, CurrentDeviceInfo!.ID).Wait();
        //            _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Sidetone ...................= {deviceInfoDTP.Sidetone.ToString()}");
        //        }
        //        else
        //        {
        //            deviceInfoDTP.IsSidetoneSupported = false;
        //            _log.Info($"[AirAudioViewModel] DTH GetIsSidetoneSupportedAsync ......................... NO");
        //        }
        //        //------------------------------------------------------------------------------------

        //        if (CurrentDeviceInfo!.IsVoiceGuidanceSupported)
        //        {
        //            deviceInfoDTP.IsVoiceGuidanceSupported = true;
        //            _deviceManager.SetVoiceGuidance(true, CurrentDeviceInfo!.ID).Wait();
        //            _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.VoiceGuidance ..............= {deviceInfoDTP.VoiceGuidance.ToString()}");
        //        }
        //        else
        //        {
        //            deviceInfoDTP.IsVoiceGuidanceSupported = false;
        //            _log.Info($"[AirAudioViewModel] DTH GetIsVoiceGuidanceSupportedAsync .................... NO");
        //        }
        //        //------------------------------------------------------------------------------------

        //        if (CurrentDeviceInfo!.IsPresetsSupported)
        //        {
        //            deviceInfoDTP.IsPresetsSupported = true;
        //            _deviceManager.SetSelectedPreset(deviceInfoDTP.SelectedPreset, CurrentDeviceInfo!.ID).Wait();
        //            _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.SelectedPreset .............= {deviceInfoDTP.SelectedPreset.ToString()}");
        //            if (CurrentDeviceInfo!.IsEqualizerSupported)
        //            {
        //                // DTH no Band Gain
        //                deviceInfoDTP.IsEqualizerSupported = true;
        //                _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Band1Gain ..................= {deviceInfoDTP.Band1Gain.ToString()}");
        //                _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Band2Gain ..................= {deviceInfoDTP.Band2Gain.ToString()}");
        //                _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Band3Gain ..................= {deviceInfoDTP.Band3Gain.ToString()}");
        //                _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Band4Gain ..................= {deviceInfoDTP.Band4Gain.ToString()}");
        //                _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Band5Gain ..................= {deviceInfoDTP.Band5Gain.ToString()}");
        //            }
        //            else
        //            {
        //                deviceInfoDTP.IsEqualizerSupported = false;
        //                _log.Info($"[AirAudioViewModel] DTH GetIsEqualizerSupportedAsync .................... NO");
        //            }
        //        }
        //        else
        //        {
        //            deviceInfoDTP.IsPresetsSupported = false;
        //            _log.Info($"[AirAudioViewModel] DTH GetIsPresetsSupportedAsync ................... NO");
        //        }
        //        //------------------------------------------------------------------------------------

        //        if (CurrentDeviceInfo!.IsMicNCIncomingSupported)
        //        {
        //            deviceInfoDTP.IsMicNCIncomingSupported = true;
        //            _deviceManager.SetMicNCIncoming(deviceInfoDTP.MicNCIncoming, CurrentDeviceInfo!.ID).Wait();
        //            _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.MicNCIncoming .............= {deviceInfoDTP.MicNCIncoming.ToString()}");
        //        }
        //        else
        //        {
        //            _log.Info($"[AirAudioViewModel] DTH GetIsMicNCIncomingSupportedAsync ............. NO");
        //        }
        //        //------------------------------------------------------------------------------------
        //    }
        //    catch (Exception ex)
        //    {
        //        _log!.Error($"[AirAudioViewModel] DTH SetFactoryResetForDTH ...... {ex.ToString()}");
        //    }
        //}

        /// <summary>
        /// Update DTP AirAudio property Value
        /// </summary>
        /// <returns></returns>
        private void UpdateDTPValue()
        {
            try
            {
                _log.Info($"[AirAudioViewModel] DTP Print before property ...UpdateDTPValue ... in");

                if (deviceInfoDTP == null)
                {
                    deviceInfoDTP = new DeviceInfoDTP();
                    _log.Info($"[AirAudioViewModel] DTP Print before property ...UpdateDTPValue new DeviceInfo...");
                }

                _log.Info($"[AirAudioViewModel] DTP Start print and read property ......");
                _log.Info($"[AirAudioViewModel] ***********************************************************************");

                //object varr = await _deviceManager.GetAirAudioDeviceItemsExAsync();
                bool IsAnC = _deviceManager.GetAirAudioIsANCSupportedAsync(CurrentDeviceID.ToString()).Result;
                //Read this AirAudio Support function
                if (IsAnC)
                {
                    deviceInfoDTP.IsANCSupported = true;

                    deviceInfoDTP.AncMode = _deviceManager.GetAirAudioAncModeAsync(CurrentDeviceID.ToString()).Result;
                    deviceInfoDTP.AncGain = _deviceManager.GetAirAudioAncGainAsync(CurrentDeviceID.ToString()).Result;

                    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.AncMode .....................= {deviceInfoDTP.AncMode.ToString()}");
                    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.AncGain .....................= {deviceInfoDTP.AncGain.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsANCSupported = false;
                    deviceInfoDTP.AncMode = 0;
                    deviceInfoDTP.AncGain = 0;
                    _log.Info($"[AirAudioViewModel] DTP GetIsANCSupportedAsync .............................. NO");
                }
                bool IsBusyLigh = _deviceManager.GetAirAudioIsBusyLightSupportedAsync(CurrentDeviceID.ToString()).Result;
                //------------------------------------------------------------------------------------
                if (IsBusyLigh)
                {
                    deviceInfoDTP.IsBusyLightSupported = true;
                    deviceInfoDTP.BusyLight = _deviceManager.GetAirAudioBusyLightAsync(CurrentDeviceID.ToString()).Result;

                    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.BusyLight ...................= {deviceInfoDTP.BusyLight.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsBusyLightSupported = false;
                    deviceInfoDTP.BusyLight = false;
                    _log.Info($"[AirAudioViewModel] DTP GetIsBusyLightSupportedAsync ........................ NO");
                }
                //------------------------------------------------------------------------------------
                bool IsMicNoiseCancellationSupported = _deviceManager.GetAirAudioIsMicNoiseCancellationSupportedAsync(CurrentDeviceID.ToString()).Result;
                if (IsMicNoiseCancellationSupported)
                {
                    deviceInfoDTP.IsMicNoiseCancellationSupported = true;
                    //if (isAirAudio == false)
                    //{
                    deviceInfoDTP.MicNoiseCancellation = _deviceManager.GetAirAudioMicNoiseCancellationAsync(CurrentDeviceID.ToString()).Result;
                    //}

                    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.MicNoiseCancellation .............= {deviceInfoDTP.MicNoiseCancellation.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsMicNoiseCancellationSupported = false;
                    deviceInfoDTP.MicNoiseCancellation = false;
                    _log.Info($"[AirAudioViewModel] DTP GetIsMicNoiseCancellationSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------
                bool IsSidetoneSupported = _deviceManager.GetAirAudioIsSidetoneSupportedAsync(CurrentDeviceID.ToString()).Result;
                if (IsSidetoneSupported)
                {
                    deviceInfoDTP.IsSidetoneSupported = true;

                    deviceInfoDTP.Sidetone = _deviceManager.GetAirAudioSidetoneAsync(CurrentDeviceID.ToString()).Result;

                    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.Sidetone ...................= {deviceInfoDTP.Sidetone.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsSidetoneSupported = false;
                    deviceInfoDTP.Sidetone = false;
                    _log.Info($"[AirAudioViewModel] DTP GetIsSidetoneSupportedAsync ......................... NO");
                }
                //------------------------------------------------------------------------------------
                bool IsVoiceGuidanceSupported = _deviceManager.GetAirAudioIsVoiceGuidanceSupportedAsync(CurrentDeviceID.ToString()).Result;
                if (IsVoiceGuidanceSupported)
                {
                    deviceInfoDTP.IsVoiceGuidanceSupported = true;
                    deviceInfoDTP.VoiceGuidance = _deviceManager.GetAirAudioVoiceGuidanceAsync(CurrentDeviceID.ToString()).Result;

                    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.VoiceGuidance ..............= {deviceInfoDTP.VoiceGuidance.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsVoiceGuidanceSupported = false;
                    deviceInfoDTP.VoiceGuidance = false;
                    _log.Info($"[AirAudioViewModel] DTP GetIsVoiceGuidanceSupportedAsync .................... NO");
                }
                //------------------------------------------------------------------------------------
                bool IsPresetsSupported = _deviceManager.GetAirAudioIsPresetsSupportedAsync(CurrentDeviceID.ToString()).Result;
                if (IsPresetsSupported)
                {
                    deviceInfoDTP.IsPresetsSupported = true;

                    deviceInfoDTP.SelectedPreset = _deviceManager.GetAirAudioSelectedPresetAsync(CurrentDeviceID.ToString()).Result;

                    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.SelectedPreset .............= {deviceInfoDTP.SelectedPreset.ToString()}");
                    bool IsEqualizerSupported = _deviceManager.GetAirAudioIsEqualizerSupportedAsync(CurrentDeviceID.ToString()).Result;
                    if (IsEqualizerSupported)
                    {
                        deviceInfoDTP.IsEqualizerSupported = true;

                        deviceInfoDTP.Band1Gain = _deviceManager.GetAirAudioBand1GainAsync(CurrentDeviceID.ToString()).Result;
                        deviceInfoDTP.Band2Gain = _deviceManager.GetAirAudioBand2GainAsync(CurrentDeviceID.ToString()).Result;
                        deviceInfoDTP.Band3Gain = _deviceManager.GetAirAudioBand3GainAsync(CurrentDeviceID.ToString()).Result;
                        deviceInfoDTP.Band4Gain = _deviceManager.GetAirAudioBand4GainAsync(CurrentDeviceID.ToString()).Result;
                        deviceInfoDTP.Band5Gain = _deviceManager.GetAirAudioBand5GainAsync(CurrentDeviceID.ToString()).Result;

                        _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.Band1Gain ..................= {deviceInfoDTP.Band1Gain.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.Band2Gain ..................= {deviceInfoDTP.Band2Gain.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.Band3Gain ..................= {deviceInfoDTP.Band3Gain.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.Band4Gain ..................= {deviceInfoDTP.Band4Gain.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.Band5Gain ..................= {deviceInfoDTP.Band5Gain.ToString()}");
                    }
                    else
                    {
                        deviceInfoDTP.IsEqualizerSupported = false;
                        _log.Info($"[AirAudioViewModel] DTP GetIsEqualizerSupportedAsync .................... NO");
                    }
                }
                else
                {
                    deviceInfoDTP.IsPresetsSupported = false;
                    deviceInfoDTP.IsEqualizerSupported = false;
                    _log.Info($"[AirAudioViewModel] DTP GetIsPresetsSupportedAsync ................... NO");
                }
                //------------------------------------------------------------------------------------
                bool IsMicNCIncomingSupported = _deviceManager.GetAirAudioIsMicNCIncomingSupportedAsync(CurrentDeviceID.ToString()).Result;
                if (IsMicNCIncomingSupported)
                {
                    deviceInfoDTP.IsMicNCIncomingSupported = true;

                    deviceInfoDTP.MicNCIncoming = _deviceManager.GetAirAudioMicNCIncomingAsync(CurrentDeviceID.ToString()).Result;

                    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.MicNCIncoming .............= {deviceInfoDTP.MicNCIncoming.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsMicNCIncomingSupported = false;
                    deviceInfoDTP.MicNCIncoming = false;
                    _log.Info($"[AirAudioViewModel] DTP GetIsMicNCIncomingSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------
                //bool IsWearDetectionSupported = isAirAudio == false ? _deviceManager.GetIsWearDetectionSupportedAsync(CurrentDeviceID.ToString()).Result : false;
                //if (IsWearDetectionSupported)
                //{
                //    deviceInfoDTP.IsWearDetectionSupported = true;
                //    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.GetIsWearDetectionSupportedAsync .............= {deviceInfoDTP.IsWearDetectionSupported.ToString()}");
                //    if (deviceInfoDTP.IsWearDetectionSupported)
                //    {
                //        deviceInfoDTP.WearDetectionFromDTP = _deviceManager.GetWearDetectionAsync(CurrentDeviceID.ToString()).Result;
                //        _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.WearDetectionFromDTP .............= {deviceInfoDTP.WearDetectionFromDTP.ToString()}");

                //        deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = _deviceManager.GetIsWearDetectionPauseMusicEnabledAsync(CurrentDeviceID.ToString()).Result;
                //        _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP .............= {deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");

                //        deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = _deviceManager.GetIsWearDetectionMuteMicEnabledAsync(CurrentDeviceID.ToString()).Result;
                //        _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP .............= {deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");

                //        deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = _deviceManager.GetWearDetectionQuickPauseAsync(CurrentDeviceID.ToString()).Result;
                //        _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP .............= {deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");

                //        deviceInfoDTP.WearDetectionSensitivityFromDTP = _deviceManager.GetWearDetectionSensitivityAsync(CurrentDeviceID.ToString()).Result;
                //        _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.WearDetectionSensitivityFromDTP .............= {deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
                //    }
                //}
                //else
                //{
                //    deviceInfoDTP.IsWearDetectionSupported = false;
                //    deviceInfoDTP.WearDetectionFromDTP = false;
                //    _log.Info($"[AirAudioViewModel] DTP GetIsWearDetectionSupportedAsync ............. NO");
                //}
                //------------------------------------------------------------------------------------
                bool IsBoomMicSupported = _deviceManager.GetAirAudioIsBoomMicSupportedAsync(CurrentDeviceID.ToString()).Result;
                if (IsBoomMicSupported)
                {
                    _supportedAnswerCalls = true;
                    deviceInfoDTP.IsAnswerCallSupported = true;
                    deviceInfoDTP.AnswerCall = _deviceManager.GetBoomMicAsync(CurrentDeviceID.ToString()).Result;
                    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.AnswerCall .............= {deviceInfoDTP.AnswerCall.ToString()}");
                }
                else
                {
                    _supportedAnswerCalls = false;
                    deviceInfoDTP.IsAnswerCallSupported = false;
                    deviceInfoDTP.AnswerCall = false;
                    _log.Info($"[AirAudioViewModel] DTP deviceInfoDTP.AnswerCall ............. NO");
                }
                //------------------------------------------------------------------------------------

                //deviceInfoDTP.BatteryLevel = await _deviceManager.GetAirAudioBatteryLevelAsync(CurrentDeviceID.ToString());
                deviceInfoDTP.SidetoneLevel = _deviceManager.GetAirAudioSidetoneLevelAsync(CurrentDeviceID.ToString()).Result;
                //if (isAirAudio == false)
                //{
                //    PairedHostName1 = _deviceManager.GetAirAudioPairedHostName2Async(CurrentDeviceInfo.ID.ToString()).Result;
                //    PairedHostName2 = _deviceManager.GetAirAudioPairedHostName3Async(CurrentDeviceInfo.ID.ToString()).Result;
                //}

                UpdateResetToDefault();
                //OnPropertyChanged(nameof(IsRestoreEnable));
            }
            catch (Exception ex)
            {
                _log!.Error($"[AirAudioViewModel]DTP  UpdateDTPValue ...... {ex.ToString()}");
            }
        }

        /// <summary>
        /// Update DTH AirAudio property Value
        /// </summary>
        /// <returns></returns>
        private void UpdateDTHValue()
        {
            try
            {
                _log.Info($"[AirAudioViewModel] DTH Print before property ...UpdateDTHValue ... in");

                if (deviceInfoDTP == null)
                {
                    deviceInfoDTP = new DeviceInfoDTP();
                    _log.Info($"[AirAudioViewModel] DTH Print before property ...UpdateDTPValue new DeviceInfo...");
                }

                _log.Info($"[AirAudioViewModel] DTH Print after property ......");
                _log.Info($"[AirAudioViewModel] DTH ***********************************************************************");

                //object varr = await _deviceManager.GetAirAudioDeviceItemsExAsync();

                //Read this AirAudio Support function
                if (CurrentDeviceInfo!.IsANCSupported)
                {
                    deviceInfoDTP.IsANCSupported = true;
                    deviceInfoDTP.AncMode = CurrentDeviceInfo!.AncMode;
                    deviceInfoDTP.AncGain = CurrentDeviceInfo!.AncGain;
                    _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.AncMode .....................= {deviceInfoDTP.AncMode.ToString()}");
                    _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.AncGain .....................= {deviceInfoDTP.AncGain.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsANCSupported = false;
                    deviceInfoDTP.AncMode = 0;
                    deviceInfoDTP.AncGain = 0;
                    _log.Info($"[AirAudioViewModel] DTH GetIsANCSupportedAsync .............................. NO");
                }
                //------------------------------------------------------------------------------------
                if (CurrentDeviceInfo!.IsBusyLightSupported)
                {
                    deviceInfoDTP.IsBusyLightSupported = true;
                    deviceInfoDTP.BusyLight = CurrentDeviceInfo!.BusyLight;
                    _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.BusyLight ...................= {deviceInfoDTP.BusyLight.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsBusyLightSupported = false;
                    deviceInfoDTP.BusyLight = false;
                    _log.Info($"[AirAudioViewModel] DTH GetIsBusyLightSupportedAsync ........................ NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsMicNoiseCancellationSupported)
                {
                    deviceInfoDTP.IsMicNoiseCancellationSupported = true;
                    deviceInfoDTP.MicNoiseCancellation = CurrentDeviceInfo!.MicNoiseCancellation;
                    _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.MicNoiseCancellation .............= {deviceInfoDTP.MicNoiseCancellation.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsMicNoiseCancellationSupported = false;
                    deviceInfoDTP.MicNoiseCancellation = false;
                    _log.Info($"[AirAudioViewModel] DTH GetIsMicNoiseCancellationSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsSidetoneSupported)
                {
                    deviceInfoDTP.IsSidetoneSupported = true;
                    deviceInfoDTP.Sidetone = CurrentDeviceInfo!.Sidetone;
                    _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Sidetone ...................= {deviceInfoDTP.Sidetone.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsSidetoneSupported = false;
                    deviceInfoDTP.Sidetone = false;
                    _log.Info($"[AirAudioViewModel] DTH GetIsSidetoneSupportedAsync ......................... NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsVoiceGuidanceSupported)
                {
                    deviceInfoDTP.IsVoiceGuidanceSupported = true;
                    deviceInfoDTP.VoiceGuidance = CurrentDeviceInfo!.VoiceGuidance;
                    _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.VoiceGuidance ..............= {deviceInfoDTP.VoiceGuidance.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsVoiceGuidanceSupported = false;
                    deviceInfoDTP.VoiceGuidance = false;
                    _log.Info($"[AirAudioViewModel] DTH GetIsVoiceGuidanceSupportedAsync .................... NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsPresetsSupported)
                {
                    deviceInfoDTP.IsPresetsSupported = true;
                    deviceInfoDTP.SelectedPreset = CurrentDeviceInfo!.SelectedPreset;
                    _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.SelectedPreset .............= {deviceInfoDTP.SelectedPreset.ToString()}");
                    if (CurrentDeviceInfo!.IsEqualizerSupported)
                    {
                        deviceInfoDTP.IsEqualizerSupported = true;
                        deviceInfoDTP.Band1Gain = CurrentDeviceInfo!.Band1Gain;
                        deviceInfoDTP.Band2Gain = CurrentDeviceInfo!.Band2Gain;
                        deviceInfoDTP.Band3Gain = CurrentDeviceInfo!.Band3Gain;
                        deviceInfoDTP.Band4Gain = CurrentDeviceInfo!.Band4Gain;
                        deviceInfoDTP.Band5Gain = CurrentDeviceInfo!.Band5Gain;
                        _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Band1Gain ..................= {deviceInfoDTP.Band1Gain.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Band2Gain ..................= {deviceInfoDTP.Band2Gain.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Band3Gain ..................= {deviceInfoDTP.Band3Gain.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Band4Gain ..................= {deviceInfoDTP.Band4Gain.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.Band5Gain ..................= {deviceInfoDTP.Band5Gain.ToString()}");
                    }
                    else
                    {
                        deviceInfoDTP.IsEqualizerSupported = false;
                        _log.Info($"[AirAudioViewModel] DTH GetIsEqualizerSupportedAsync .................... NO");
                    }
                }
                else
                {
                    deviceInfoDTP.IsPresetsSupported = false;
                    deviceInfoDTP.IsEqualizerSupported = false;
                    _log.Info($"[AirAudioViewModel] DTH GetIsPresetsSupportedAsync ................... NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsMicNCIncomingSupported)
                {
                    deviceInfoDTP.IsMicNCIncomingSupported = true;
                    deviceInfoDTP.MicNCIncoming = CurrentDeviceInfo!.MicNCIncoming;
                    _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.MicNCIncoming .............= {deviceInfoDTP.MicNCIncoming.ToString()}");
                }
                else
                {
                    deviceInfoDTP.IsMicNCIncomingSupported = false;
                    deviceInfoDTP.MicNCIncoming = false;
                    _log.Info($"[AirAudioViewModel] DTH GetIsMicNCIncomingSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsWearDetectionSupported)
                {
                    deviceInfoDTP.IsWearDetectionSupported = true;
                    _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.GetIsWearDetectionSupportedAsync .............= {deviceInfoDTP.IsWearDetectionSupported.ToString()}");
                    if (deviceInfoDTP.IsWearDetectionSupported)
                    {
                        uint wearDetectionValue = (uint)CurrentDeviceInfo!.WearDetection;
                        if (GetBitValue(wearDetectionValue, 0) == 1)
                            deviceInfoDTP.WearDetectionFromDTP = true;
                        else
                            deviceInfoDTP.WearDetectionFromDTP = false;

                        if (GetBitValue(wearDetectionValue, 1) == 1)
                            deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = true;
                        else
                            deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = false;

                        if (GetBitValue(wearDetectionValue, 2) == 1)
                            deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = true;
                        else
                            deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = false;

                        if (GetBitValue(wearDetectionValue, 4) == 1)
                            deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 1;
                        else
                            deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 0;

                        //7024
                        if (CurrentDeviceInfo!.ModelNumber.Contains("7024"))
                        {
                            if (GetBitsValue(wearDetectionValue, 5) == 1)
                            {
                                deviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
                                deviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
                            }
                            else
                            {
                                deviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
                                deviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
                            }
                        }

                        //5024
                        if (CurrentDeviceInfo!.ModelNumber.Contains("5024"))
                        {
                            if (GetBitValue(wearDetectionValue, 3) == 1)
                            {
                                deviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
                                deviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
                            }
                            else
                            {
                                deviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
                                deviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
                            }
                        }
                        _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.WearDetectionFromDTP .............= {deviceInfoDTP.WearDetectionFromDTP.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP .............= {deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP .............= {deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP .............= {deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
                        _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.WearDetectionSensitivityFromDTP .............= {deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
                    }
                }
                else
                {
                    deviceInfoDTP.IsWearDetectionSupported = false;
                    deviceInfoDTP.WearDetectionFromDTP = false;
                    deviceInfoDTP.WearDetection = 0;
                    _log.Info($"[AirAudioViewModel] DTH GetIsWearDetectionSupportedAsync ............. NO");
                }
                //-----------------------------------------------------------------------------------------
                if (CurrentDeviceInfo.FirmwareVersion != "" && CurrentDeviceInfo.FirmwareVersion != string.Empty)
                {
                    string resultFW = CurrentDeviceInfo.FirmwareVersion.Replace(".", "");
                    int fwv = int.Parse(resultFW);
                    if (Model == "WH3024" || Model == "WL3024" || Model == "WH5024")
                    {
                        int fwvThreshold = 0;

                        switch (Model)
                        {
                            case "WH3024":
                                fwvThreshold = 278;
                                break;
                            case "WH5024":
                                fwvThreshold = 227;
                                break;
                            case "WL3024":
                                fwvThreshold = 1104;
                                break;
                        }

                        if (fwv > fwvThreshold)
                        {
                            _supportedAnswerCalls = true;
                            deviceInfoDTP.IsAnswerCallSupported = true;
                            deviceInfoDTP.AnswerCall = true;
                            _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.AnswerCall .............= {deviceInfoDTP.AnswerCall.ToString()}");
                        }
                        else
                        {
                            _supportedAnswerCalls = false;
                            deviceInfoDTP.IsAnswerCallSupported = false;
                            deviceInfoDTP.AnswerCall = false;
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                AirAudioGroupChanged?.Invoke(this, EventArgs.Empty);
                            });
                            _log.Info($"[AirAudioViewModel] DTH deviceInfoDTP.AnswerCall ............. NO");
                        }
                    }
                }

                deviceInfoDTP.SidetoneLevel = CurrentDeviceInfo!.SidetoneLevel;

                UpdateResetToDefault();
                //OnPropertyChanged(nameof(IsRestoreEnable));
            }
            catch (Exception ex)
            {
                _log!.Error($"[AirAudioViewModel] DTH UpdateDTPValue ...... {ex.ToString()}");
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


        public readonly string _regPath = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Global\QRCode";
        public string RegPath
        {
            get => _regPath;
        }

        private string _regKeyForQRCode = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.AirAudioQRCode.";
        public string RegKeyForQRCode
        {
            get => _regKeyForQRCode + Model;
        }
        bool IntToBool(int value) => value != 0;
        bool IntToBoolElse(int value) => value == 0;
        int BoolToInt(bool value) => value ? 1 : 0;
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
            if(IsPleaseWaitVisible==true)
                IsPleaseWaitVisible = false;
        }

        // Please Wait logic
        //public void Invoke_PleaseWait(string model, AirAudioViewModel vm)
        //{
        //    BackgroundWorker bw = new BackgroundWorker
        //    {
        //        WorkerReportsProgress = false,
        //        WorkerSupportsCancellation = false
        //    };
        //    bw.DoWork += (sender, e) => DoWork_PleaseWait(model, vm);
        //    bw.RunWorkerCompleted += RunWorkerCompleted_PleaseWait;

        //    ShowPleaseWait();
        //    bw.RunWorkerAsync();
        //}

        private void DoWork_PleaseWait(string model, AirAudioViewModel vm)
        {
            _log.Info($"[AirAudioViewModel] DoWork_PleaseWait .......");
            if (_deviceManager == null) 
            {
                HidePleaseWait();
                _log.Error("[AirAudioViewModel] DoWork_PleaseWait ... _deviceManager is null.");
                return;
            }
            if (CurrentDeviceID == null || string.IsNullOrEmpty(CurrentDeviceID.ToString()))
            {
                HidePleaseWait();
                _log.Error("[AirAudioViewModel] DoWork_PleaseWait ... CurrentDeviceID is null.");
                return;
            }
            DeviceID = _deviceManager.GetAirAudioSerialNumberAsync(CurrentDeviceID.ToString()).Result;
            if (string.IsNullOrEmpty(DeviceID))
            {
                _log.Error("[AirAudioViewModel] DoWork_PleaseWait ... DeviceID is null.");
                // Handle the null case appropriately, e.g., set a default value or return
                DeviceID = "Unknown DeviceID";
            }


            FirmwareVersion2 = _deviceManager.GetAirAudioFirmwareVersionAsync(CurrentDeviceID.ToString()).Result;
            if (_deviceManager.GetDTPProxyPluginReady().Result)
            {
                IsDTPReady = true;
                _log.Info($"[AirAudioViewModel] DoWork_PleaseWait ... GetDTPProxyPluginReady, true ...");
            }
            else
            {
                IsDTPReady = false;
                _log.Info($"[AirAudioViewModel] DoWork_PleaseWait ... GetDTPProxyPluginReady, false ...");
            }
            if (!_waitAirAudioReady) //If false, re-get once time
                _waitAirAudioReady = _deviceManager.GetAirAudioIsReadyAsync(CurrentDeviceID.ToString()).Result;
            //Thread.Sleep(500);
            //if ((FirmwareVersion2 == null || FirmwareVersion2 == "0.0.0.0") || !_waitAirAudioReady)
            if (!_waitAirAudioReady)
            {
                int tick = 0;
                while (!_deviceManager.GetAirAudioIsReadyAsync(CurrentDeviceID.ToString()).Result)
                {
                    _log.Info($"[AirAudioViewModel] DoWork_PleaseWait ... GetIsReadyAsync, false ... {tick.ToString()}");
                    if (tick >= 20) // 20 sec force exit
                    {
                        _log.Info($"[AirAudioViewModel] DoWork_PleaseWait ... Can not get AirAudioReady ...... {tick.ToString()} sec, fail ...");
                        break;
                    }
                    Task.Delay(1000).Wait();//Thread.Sleep(1000); // IL provide info, DPeM 18 need 3sec, DPeM 20 need 18~25 sec,
                    tick++;
                }
                if (_waitAirAudioReady)
                {
                    FirmwareVersion2 = _deviceManager.GetAirAudioFirmwareVersionAsync(CurrentDeviceID.ToString()).Result;
                    DeviceID = _deviceManager.GetAirAudioSerialNumberAsync(CurrentDeviceID.ToString()).Result;
                    if (string.IsNullOrEmpty(DeviceID))
                    {
                        _log.Error("[AirAudioViewModel] DoWork_PleaseWait ... DeviceID is null.");
                        // Handle the null case appropriately, e.g., set a default value or return
                        DeviceID = "Unknown DeviceID";
                    }

                    if (FirmwareVersion2 != null && FirmwareVersion2 != "0.0.0.0")
                    {
                        FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";
                        DeviceID = Strings.DeviceID + $" {DeviceID}";
                        _waitAirAudioReady = true;
                        _log.Info($"[AirAudioViewModel] DoWork_PleaseWait ... Get waitAirAudioReady event True, {FirmwareVersion2} ...... ");
                    }
                    else
                    {
                        FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";
                        DeviceID = Strings.DeviceID + $" {DeviceID}";
                        _log.Info($"[AirAudioViewModel] DoWork_PleaseWait ... Get waitAirAudioReady event True, but {FirmwareVersion2} ...... ");
                    }
                }
            }
            else
            {
                _log.Info($"[AirAudioViewModel] DoWork_PleaseWait ... GetAirAudioFirmwareVersionAsync ...... DTP success ...");
                _waitAirAudioReady = true;
                if (!FirmwareVersion2.Contains(Strings.FirmwareVersion))
                    FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";
                if (!DeviceID.Contains(Strings.DeviceID))
                    DeviceID = Strings.DeviceID + $" {DeviceID}";
                _log.Info($"[AirAudioViewModel] DoWork_PleaseWait ... Firmware Version from DTP ... {FirmwareVersion2} ...");
            }
            if (IsDTPReady)
                UpdateDTPValue();
            else
                UpdateDTHValue();
            // Call DetectPageShow
            DetectPageShow(model);
            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //{
            //    AirAudioGroupChanged?.Invoke(this, EventArgs.Empty);
            //});
            Task.Delay(500).Wait();//Thread.Sleep(500);
            HidePleaseWait();
        }

        public async Task Invoke_PleaseWaitAsync(string model, AirAudioViewModel vm)
        {
            vm.ShowPleaseWait();
            try
            {
                //using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
                //{
                await Task.Run(() => DoWork_PleaseWait(model, vm));
                //}
            }
            catch (OperationCanceledException)
            {
                _log!.Error("[AirAudioViewModel] Invoke_PleaseWaitAsync timed out");
                await Task.Run(() => HidePleaseWait());
                throw;
            }
            catch (Exception ex)
            {
                vm._log!.Error($"[AirAudioViewModel] Invoke_PleaseWaitAsync exception: {ex.Message}");
                await Task.Run(() => HidePleaseWait());
                throw;
            }
            //finally
            //{
            //    vm.HidePleaseWait();
            //}
        }

        private void RunWorkerCompleted_PleaseWait(object sender, RunWorkerCompletedEventArgs e)
        {
            HidePleaseWait();
        }

        //public bool _waitAirAudioFW = false;
        //public bool waitAirAudioFW
        //{
        //    get
        //    {
        //        return _waitAirAudioFW;
        //    }
        //    set
        //    {
        //        _waitAirAudioFW = value;
        //    }
        //}

        //public bool _waitAirAudioReady = false;
        //public bool waitAirAudioReady
        //{
        //    get
        //    {
        //        return _waitAirAudioReady;
        //    }
        //    set
        //    {
        //        _waitAirAudioReady = value;
        //    }
        //}
        #endregion Please Wait

        /// <summary>
        /// AirAudioAudioSettings Page
        /// </summary>

        //private Visibility _isAncLockMask = Visibility.Collapsed;

        //public Visibility IsAncLockMask
        //{
        //    get { return _isAncLockMask; }
        //    set
        //    {
        //        if (_isAncLockMask != value)
        //        {
        //            _isAncLockMask = value;
        //            OnPropertyChanged(nameof(_isAncLockMask));
        //        }
        //    }
        //}

        //public bool IsRestoreEnable { get; set; } = false;

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
        #region AirAudioAudioSettings ToggleSwitch Binding

        //Outgoing Audio ToggleSwitch
        private string _isOutgoingAudio_String = Strings.On;

        public string OutgoingAudio_String
        {
            get => _isOutgoingAudioStatus ? Strings.On : Strings.Off;
        }

        private bool _supportedOutgoingAudio = true;

        public bool SupportedOutgoingAudio
        {
            get
            {
                return _supportedOutgoingAudio;
            }
            set
            {
                _supportedOutgoingAudio = value;
                OnPropertyChanged(nameof(SupportedOutgoingAudio));
            }
        }

        private bool _isOutgoingAudioStatus = false;

        public bool OutgoingAudioStatus
        {
            get
            {
                _isOutgoingAudioStatus = deviceInfoDTP!.MicNoiseCancellation;
                //if (isAirAudio)
                //{
                //    return true;
                //}
                return _isOutgoingAudioStatus;
            }
            set
            {
                _isRestoreEnable = false;
                deviceInfoDTP!.MicNoiseCancellation = value;
                _isOutgoingAudioStatus = value;
                _supportedOutgoingAudio = false;

                _deviceManager.SetAirAudioMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.MicNoiseCancellation).Wait();

                _supportedOutgoingAudio = true;
                //_debouncerAirAudio.Debounce("OutgoingAudioCheck");
                OnPropertyChanged(nameof(OutgoingAudio_String));
                _log.Info($"[AirAudioViewModel] SetMicNoiseCancellationAsync ... OutgoingAudioCheck .... {deviceInfoDTP.MicNoiseCancellation.ToString()}");
            }
        }

        //Incoming Audio ToggleSwitch
        private string _isIncomingAudio_String = Strings.On;

        public string IncomingAudio_String
        {
            get => _isIncomingAudioStatus ? Strings.On : Strings.Off;
        }

        private bool _supportedIncomingAudio = true;

        public bool SupportedIncomingAudio
        {
            get
            {
                return _supportedIncomingAudio;
            }
            set
            {
                _supportedIncomingAudio = value;
                OnPropertyChanged(nameof(SupportedIncomingAudio));
            }
        }

        private bool _isIncomingAudioStatus = false;

        public bool IncomingAudioStatus
        {
            get
            {
                _isIncomingAudioStatus = deviceInfoDTP!.MicNCIncoming;
                return _isIncomingAudioStatus;
            }
            set
            {
                _isRestoreEnable = false;
                deviceInfoDTP!.MicNCIncoming = value;
                _isIncomingAudioStatus = value;
                _supportedIncomingAudio = false;
                if (IsDTPReady)
                {
                    _deviceManager.SetAirAudioMicNCIncomingAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.MicNCIncoming).Wait();
                }
                else
                    _deviceManager.SetMicNCIncoming(value, CurrentDeviceInfo!.ID).Wait();
                //_debouncerAirAudio.Debounce("IncomingAudioCheck");
                _supportedIncomingAudio = true;
                OnPropertyChanged(nameof(IncomingAudio_String));
                _log.Info($"[AirAudioViewModel] SetMicNCIncomingAsync ... IncomingAudioCheck .... {deviceInfoDTP.MicNCIncoming.ToString()}");
            }
        }

        //MicNoiseCancellation ToggleSwitch
        private string _isMicNoiseCancellation_String = Strings.On;

        public string MicNoiseCancellation_String
        {
            get => _isMicNoiseCancellationStatus ? Strings.On : Strings.Off;
        }

        private bool _supportedMicNoiseCancellation = true;

        public bool SupportedMicNoiseCancellation
        {
            get
            {
                return _supportedMicNoiseCancellation = deviceInfoDTP!.IsMicNoiseCancellationSupported;
            }
            set
            {
                _supportedMicNoiseCancellation = value;
                OnPropertyChanged(nameof(_supportedMicNoiseCancellation));
            }
        }

        private bool _isMicNoiseCancellationStatus;

        public bool MicNoiseCancellationStatus
        {
            get
            {
                _isMicNoiseCancellationStatus = deviceInfoDTP!.MicNoiseCancellation;
                return _isMicNoiseCancellationStatus;
            }
            set
            {
                _isRestoreEnable = false;
                deviceInfoDTP!.MicNoiseCancellation = value;
                _isMicNoiseCancellationStatus = value;
                _supportedMicNoiseCancellation = false;
                if (IsDTPReady)
                {
                    _deviceManager.SetAirAudioMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.MicNoiseCancellation).Wait();
                }
                else
                    _deviceManager.SetMicNoiseCancellation(value, CurrentDeviceInfo!.ID).Wait();
                //_debouncerAirAudio.Debounce("MicNoiseCancellationCheck");
                _supportedMicNoiseCancellation = true;
                OnPropertyChanged(nameof(MicNoiseCancellation_String));
                _log.Info($"[AirAudioViewModel] SetMicNoiseCancellationAsync ... MicNoiseCancellationCheck .... {deviceInfoDTP.MicNoiseCancellation.ToString()}");
            }
        }

        //Sidetone ToggleSwitch
        private string _isSidetone_String = "ON";

        public string Sidetone_String
        {
            get => _isSidetoneStatus ? Strings.On : Strings.Off;
        }

        private bool _isSidetoneStatus = true;// = false;

        public bool SidetoneStatus
        {
            get
            {
                _isSidetoneStatus = deviceInfoDTP!.Sidetone;
                return _isSidetoneStatus;
            }
            set
            {
                _isRestoreEnable = false;
                deviceInfoDTP!.Sidetone = value;
                _isSidetoneStatus = value;
                if (IsDTPReady)
                {
                    _debouncerAirAudioSidetoneCheck.Debounce("SidetoneCheck");
                }
                else
                    _deviceManager.SetSidetone(value, CurrentDeviceInfo!.ID).Wait();
                OnPropertyChanged(nameof(Sidetone_String));
                OnPropertyChanged(nameof(SidetoneStatus));
                OnPropertyChanged(nameof(SidetoneSliderStatus));
            }
        }

        private int _isidetoneSliderValue;

        public int SidetoneSliderValue
        {
            get
            {
                _isidetoneSliderValue = deviceInfoDTP.SidetoneLevel;//_deviceManager.GetSidetoneLevelAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.SidetoneLevel;
                return _isidetoneSliderValue;
            }

            set
            {
                _isRestoreEnable = false;
                deviceInfoDTP!.SidetoneLevel = value;
                _isidetoneSliderValue = value;
                if (IsDTPReady)
                    _debouncerAirAudio.Debounce("SidetoneSlider");
                else
                    _deviceManager.SetSidetoneLevel(value, CurrentDeviceInfo!.ID).Wait();
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

        #endregion AirAudioAudioSettings ToggleSwitch Binding

        #region AirAudioAudioSettings Grid Show/Hide

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

        #endregion AirAudioAudioSettings Grid Show/Hide

        #region AirAudioAudioSettingsRightView

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
                if (_isActiveNoiseCancellingChecked == value && value == true)
                {
                    return;
                }

                if (_isActiveNoiseCancellingChecked != value && value)
                {
                    _isActiveNoiseCancellingChecked = value;
                    if (_isActiveNoiseCancellingChecked)
                    {
                        _isRestoreEnable = false;
                        deviceInfoDTP.AncMode = 1;
                        _isTransparencyChecked = false;
                        _isNoiseOffChecked = false;
                        deviceInfoDTP!.Sidetone = true;
                        _isSidetoneStatus = true;
                        if (IsDTPReady)
                        {
                            _debouncerAirAudio.Debounce("ANC");
                            //_debouncerAirAudioSidetoneCheck.Debounce("SidetoneCheck");
                        }
                        else
                        {
                            _deviceManager.SetAncMode(1, CurrentDeviceInfo!.ID).Wait();
                            //_deviceManager.SetSidetone(value, CurrentDeviceInfo!.ID).Wait();
                        }
                        //SidetoneStatus = true;
                        //_isSidetoneStatus = true;
                        OnPropertyChanged(nameof(IsTransparencyChecked));
                        OnPropertyChanged(nameof(IsNoiseOffChecked));
                        OnPropertyChanged(nameof(Sidetone_String));
                        OnPropertyChanged(nameof(SidetoneStatus));
                        OnPropertyChanged(nameof(SidetoneSliderStatus));
                    }
                }
            }
        }

        public bool IsTransparencyChecked
        {
            get => _isTransparencyChecked;
            set
            {
                if (_isTransparencyChecked == value && value == true)
                {
                    return;
                }

                if (_isTransparencyChecked != value && value)
                {
                    _isTransparencyChecked = value;
                    if (_isTransparencyChecked)
                    {
                        _isRestoreEnable = false;
                        deviceInfoDTP.AncMode = 2;
                        _isActiveNoiseCancellingChecked = false;
                        _isNoiseOffChecked = false;
                        deviceInfoDTP!.Sidetone = false;
                        //SidetoneStatus = false;
                        _isSidetoneStatus = false;
                        if (IsDTPReady)
                        {
                            _debouncerAirAudio.Debounce("Transparency");
                            //if (IsDTPReady)
                            //    _debouncerAirAudioSidetoneCheck.Debounce("SidetoneCheck");
                            //else
                            //    _deviceManager.SetSidetone(value, CurrentDeviceInfo!.ID).Wait();
                        }
                        else
                            _deviceManager.SetAncMode(2, CurrentDeviceInfo!.ID).Wait();
                        OnPropertyChanged(nameof(IsActiveNoiseCancellingChecked));
                        OnPropertyChanged(nameof(IsNoiseOffChecked));
                        OnPropertyChanged(nameof(IsTransparencyChecked));
                        OnPropertyChanged(nameof(TransparencylevelSliderValue));
                        OnPropertyChanged(nameof(Sidetone_String));
                        OnPropertyChanged(nameof(SidetoneStatus));
                        OnPropertyChanged(nameof(SidetoneSliderStatus));
                    }
                }
            }
        }

        public bool IsNoiseOffChecked
        {
            get => _isNoiseOffChecked;
            set
            {
                if (_isNoiseOffChecked == value && value == true)
                {
                    return;
                }

                if (_isNoiseOffChecked != value && value)
                {
                    _isNoiseOffChecked = value;
                    if (_isNoiseOffChecked)
                    {
                        _isRestoreEnable = false;
                        deviceInfoDTP.AncMode = 0;
                        _isActiveNoiseCancellingChecked = false;
                        _isTransparencyChecked = false;
                        deviceInfoDTP!.Sidetone = true;
                        //SidetoneStatus = true;
                        _isSidetoneStatus = true;
                        if (IsDTPReady)
                        {
                            _debouncerAirAudio.Debounce("NoiseOff");
                            //_debouncerAirAudioSidetoneCheck.Debounce("SidetoneCheck");
                        }
                        else
                        {
                            _deviceManager.SetAncMode(0, CurrentDeviceInfo!.ID).Wait();
                            //_deviceManager.SetSidetone(value, CurrentDeviceInfo!.ID).Wait();
                        }
                        OnPropertyChanged(nameof(IsActiveNoiseCancellingChecked));
                        OnPropertyChanged(nameof(IsTransparencyChecked));
                        OnPropertyChanged(nameof(Sidetone_String));
                        OnPropertyChanged(nameof(SidetoneStatus));
                        OnPropertyChanged(nameof(SidetoneSliderStatus));
                    }
                }
            }
        }

        private int _isTransparencylevelSliderValue;

        public int TransparencylevelSliderValue
        {
            get
            {
                _isTransparencylevelSliderValue = deviceInfoDTP.AncGain;//_deviceManager.GetAncGainAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.AncGain;
                return _isTransparencylevelSliderValue;
            }

            set
            {
                _isRestoreEnable = false;
                deviceInfoDTP.AncGain = value;
                _isTransparencylevelSliderValue = value;
                if (IsDTPReady)
                    _debouncerAirAudio.Debounce("TransparencylevelSlider");
                else
                    _deviceManager.SetAncGain(value, CurrentDeviceInfo!.ID).Wait();
                OnPropertyChanged(nameof(TransparencylevelSliderValue));
            }
        }

        #endregion Group 1

        //Group 2

        #region Group 2

        private bool _isCollaborationChecked = true;
        private bool _isMultimediaChecked = false;

        public bool IsCollaborationChecked
        {
            get => _isCollaborationChecked;
            set
            {
                if (_isCollaborationChecked == value && value == true)
                {
                    return;
                }

                if (_isCollaborationChecked != value && value)
                {
                    _isCollaborationChecked = value;
                    if (_isCollaborationChecked)
                    {
                        _isMultimediaChecked = false;
                        OnPropertyChanged(nameof(IsMultimediaChecked));
                    }
                }
                OnPropertyChanged(nameof(IsCollaborationChecked));// 保留需連動其他 Button
            }
        }

        public bool IsMultimediaChecked
        {
            get => _isMultimediaChecked;
            set
            {
                if (_isMultimediaChecked == value && value == true)
                {
                    return;
                }

                if (_isMultimediaChecked != value && value)
                {
                    _isMultimediaChecked = value;
                    if (_isMultimediaChecked)
                    {
                        _isCollaborationChecked = false;
                        OnPropertyChanged(nameof(IsCollaborationChecked));
                    }
                }
                OnPropertyChanged(nameof(IsMultimediaChecked));// 保留需連動其他 Button
            }
        }

        #endregion Group 2

        //Group 3

        #region Group 3

        private bool _isDefaultChecked = true;
        private bool _isBassBoostChecked = false;
        private bool _isSpeechBoostChecked = false;
        private bool _isTrebleBoostChecked = false;
        private bool _isCustomChecked = false;

        public bool IsDefaultChecked
        {
            get => _isDefaultChecked;
            set
            {
                //等於原來設定且進來的設定值為True就是重複點選
                if (_isDefaultChecked == value && value == true)
                {
                    return;
                }
                //不等於原來設定且進來的設定值為True才做
                if (_isDefaultChecked != value && value)
                {
                    _isDefaultChecked = value;
                    isAirAudioChange = false;
                    if (_isDefaultChecked)
                    {
                        _isRestoreEnable = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        if (IsDTPReady)
                        {
                            deviceInfoDTP.SelectedPreset = 1;
                        }
                        else
                        {
                            _deviceManager.SetAirAudioSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), 1).Wait();//不穩定
                        }
                        IsRestoreEnable = false;
                        OnPropertyChanged(nameof(IsDefaultChecked));
                        OnPropertyChanged(nameof(IsRestoreEnable));
                        _debouncerAirAudio.Debounce("DefaultCheck");
                        OnPropertyChanged(nameof(IsBassBoostChecked));
                        OnPropertyChanged(nameof(IsSpeechBoostChecked));
                        OnPropertyChanged(nameof(IsTrebleBoostChecked));
                        OnPropertyChanged(nameof(IsCustomChecked));
                    }
                }
            }
        }

        public bool IsBassBoostChecked
        {
            get => _isBassBoostChecked;
            set
            {
                if (_isBassBoostChecked == value && value == true)
                {
                    return;
                }

                if (_isBassBoostChecked != value && value)
                {
                    isAirAudioChange = _isBassBoostChecked = value;
                    if (_isBassBoostChecked)
                    {
                        _isRestoreEnable = false;
                        _isDefaultChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        deviceInfoDTP.SelectedPreset = 3;
                        if (IsDTPReady)
                        {
                            _debouncerAirAudio.Debounce("BassBoostCheck");
                        }
                        else
                        {
                            _deviceManager.SetAirAudioSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), 3).Wait();//不穩定
                        }
                        IsRestoreEnable = true;
                        OnPropertyChanged(nameof(IsRestoreEnable));
                        OnPropertyChanged(nameof(IsDefaultChecked));
                        OnPropertyChanged(nameof(IsSpeechBoostChecked));
                        OnPropertyChanged(nameof(IsTrebleBoostChecked));
                        OnPropertyChanged(nameof(IsCustomChecked));
                    }
                }
            }
        }

        public bool IsSpeechBoostChecked
        {
            get => _isSpeechBoostChecked;
            set
            {
                if (_isSpeechBoostChecked == value && value == true)
                {
                    return;
                }

                if (_isSpeechBoostChecked != value && value)
                {
                    isAirAudioChange = _isSpeechBoostChecked = value;
                    if (_isSpeechBoostChecked)
                    {
                        _isRestoreEnable = false;
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        deviceInfoDTP.SelectedPreset = 2;
                        if (IsDTPReady)
                        {
                            _debouncerAirAudio.Debounce("SpeechBoostCheck");
                        }
                        else
                        {
                            _deviceManager.SetAirAudioSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), 2).Wait();//不穩定
                        }

                        IsRestoreEnable = true;
                        OnPropertyChanged(nameof(IsRestoreEnable));
                        OnPropertyChanged(nameof(IsDefaultChecked));
                        OnPropertyChanged(nameof(IsBassBoostChecked));
                        OnPropertyChanged(nameof(IsTrebleBoostChecked));
                        OnPropertyChanged(nameof(IsCustomChecked));
                    }
                }
            }
        }

        public bool IsTrebleBoostChecked
        {
            get => _isTrebleBoostChecked;
            set
            {
                if (_isTrebleBoostChecked == value && value == true)
                {
                    return;
                }

                if (_isTrebleBoostChecked != value && value)
                {
                    isAirAudioChange = _isTrebleBoostChecked = value;
                    if (_isTrebleBoostChecked)
                    {
                        _isRestoreEnable = false;
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isCustomChecked = false;
                        deviceInfoDTP.SelectedPreset = 4;
                        if (IsDTPReady)
                        {
                            _debouncerAirAudio.Debounce("TrebleBoostCheck");
                        }
                        else
                        {
                            _deviceManager.SetAirAudioSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), 4).Wait();//不穩定
                        }
                        IsRestoreEnable = true;
                        OnPropertyChanged(nameof(IsRestoreEnable));
                        OnPropertyChanged(nameof(IsDefaultChecked));
                        OnPropertyChanged(nameof(IsBassBoostChecked));
                        OnPropertyChanged(nameof(IsSpeechBoostChecked));
                        OnPropertyChanged(nameof(IsCustomChecked));
                    }
                }
            }
        }

        public bool IsCustomChecked
        {
            get => _isCustomChecked;
            set
            {
                if (_isCustomChecked == value && value == true)
                {
                    return;
                }

                if (_isCustomChecked != value && value)
                {
                    isAirAudioChange = _isCustomChecked = value;
                    if (_isCustomChecked)
                    {
                        _isRestoreEnable = false;
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _audioEqualizerGridPageShow = true;
                        deviceInfoDTP.SelectedPreset = 101;
                        if (IsDTPReady)
                        {
                            _debouncerAirAudio.Debounce("CustomCheck");
                        }
                        else
                        {
                            _deviceManager.SetAirAudioSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), 101).Wait();//不穩定
                        }
                        IsRestoreEnable = true;
                        OnPropertyChanged(nameof(IsRestoreEnable));
                        OnPropertyChanged(nameof(IsDefaultChecked));
                        OnPropertyChanged(nameof(IsBassBoostChecked));
                        OnPropertyChanged(nameof(IsSpeechBoostChecked));
                        OnPropertyChanged(nameof(IsTrebleBoostChecked));
                    }
                }
                OnPropertyChanged(nameof(IsCustomChecked));
            }
        }

        #endregion Group 3

        #endregion AirAudioAudioSettingsRightView

        #region AirAudioAudioSettingsRightViewToolTip

        private string _noiseControlToolTip = Strings.HeadsetAudioSettingsToolTip_1;//"Controls the amount of external sound you hear";

        public string NoiseControlToolTip
        {
            get => _noiseControlToolTip;
        }

        private string _noiseCancellingInfoTip = Strings.HeadsetAudioSettingsToolTip_2;//"Eliminates surrounding noise";

        public string NoiseCancellingInfoTip
        {
            get => _noiseCancellingInfoTip;
        }

        private string _transparencyInfoTip = Strings.HeadsetAudioSettingsToolTip_3;//"Allows ambient sound to be heard. Adjusts the volume level of ambient sound heard.";

        public string TransparencyInfoTip
        {
            get => _transparencyInfoTip;
        }

        private string _noiseOffInfoTip = Strings.HeadsetAudioSettingsToolTip_4;//"Turns off Noise Cancellation features";

        public string NoiseOffInfoTip
        {
            get => _noiseOffInfoTip;
        }

        private string _collaborationInfoTip = Strings.HeadsetAudioSettingsToolTip_10;//"Applies when you're on a conference call";

        public string CollaborationInfoTip
        {
            get => _collaborationInfoTip;
        }

        private string _multimediaInfoTip = Strings.HeadsetAudioSettingsToolTip_11;//"Applies when you're listening to multimedia, such as music or podcasts";

        public string MultimediaInfoTip
        {
            get => _multimediaInfoTip;
        }

        private string _outgoingAudioToolTip = Strings.HeadsetAudioSettingsToolTip_5;//"Limits your near-end mic noise to create a better audio experience for others";

        public string OutgoingAudioToolTip
        {
            get => _outgoingAudioToolTip;
        }

        private string _incomingAudioToolTip = Strings.HeadsetAudioSettingsToolTip_6;//"Limits far-end mic noise to create a better audio experience for you";

        public string IncomingAudioToolTip
        {
            get => _incomingAudioToolTip;
        }

        private string _audioOutputPresetsToolTip = Strings.HeadsetAudioSettingsToolTip_7;//"Equalizer adjusts based on chosen preset";

        public string AudioOutputPresetsToolTip
        {
            get => _audioOutputPresetsToolTip;
        }

        private string _sidetoneToolTip = Strings.HeadsetAudioSettingsToolTip_8;//"Adjusts how much you can hear your own voice while speaking on a call. (Not available in Transparency mode)";

        public string SidetoneToolTip
        {
            get => _sidetoneToolTip;
        }

        private string _micNoiseCancellationToolTip = Strings.HeadsetAudioSettingsToolTip_9;//"Removes background noise to allow your voice to be heard clearly";

        public string MicNoiseCancellationToolTip
        {
            get => _micNoiseCancellationToolTip;
        }

        #endregion AirAudioAudioSettingsRightViewToolTip

        /// <summary>
        /// AirAudioAutomatedActions Page
        /// </summary>

        #region AirAudioAutomatedActions ToggleSwitch Binding

        //Wear Detection ToggleSwitch
        private string _isWearDetection_String = Strings.On;

        public string WearDetection_String
        {
            get => _isWearDetectionStatus ? Strings.On : Strings.Off;
        }

        private bool _supportedWearDetection = true;

        public bool SupportedWearDetection
        {
            get
            {
                return _supportedWearDetection;
            }
            set
            {
                _supportedWearDetection = value;
                OnPropertyChanged(nameof(SupportedWearDetection));
            }
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
                _isWearDetectionStatus = value;
                deviceInfoDTP.WearDetectionFromDTP = value;
                //_supportedWearDetection = false;
                _debouncerAirAudio.Debounce("WearDetectionCheck");
                //_deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.WearDetectionFromDTP).Wait();
                //_supportedWearDetection = true;
                OnPropertyChanged(nameof(WearDetection_String));
                _log.Info($"[AirAudioViewModel] SetWearDetectionAsync ... WearDetectionCheck ... {deviceInfoDTP.WearDetectionFromDTP.ToString()}");
            }
        }

        //PauseMusic ToggleSwitch
        private string _isPauseMusic_String = Strings.On;

        public string PauseMusic_String
        {
            get => _isPauseMusicStatus ? Strings.On : Strings.Off;
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
                if (_isMuteMicrophoneStatus == false && value == false)
                {
                    _isRestoreEnable = false;
                    _isWearDetectionStatus = false;
                    deviceInfoDTP.WearDetectionFromDTP = false;
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.WearDetectionFromDTP).Wait();
                    OnPropertyChanged(nameof(WearDetectionStatus));
                    OnPropertyChanged(nameof(WearDetection_String));
                }
                _isPauseMusicStatus = value;
                deviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = value;
                _debouncerAirAudioPauseMusic.Debounce("PauseMusicCheck");
                OnPropertyChanged(nameof(PauseMusic_String));
            }
        }

        //Mute Microphone ToggleSwitch
        private string _isMuteMicrophone_String = Strings.On;

        public string MuteMicrophone_String
        {
            get => _isMuteMicrophoneStatus ? Strings.On : Strings.Off;
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
                if (_isPauseMusicStatus == false && value == false)
                {
                    _isRestoreEnable = false;
                    _isWearDetectionStatus = false;
                    deviceInfoDTP.WearDetectionFromDTP = false;
                    _deviceManager.SetAirAudioWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.WearDetectionFromDTP).Wait();
                    OnPropertyChanged(nameof(WearDetectionStatus));
                    OnPropertyChanged(nameof(WearDetection_String));
                }
                _isMuteMicrophoneStatus = value;
                deviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = value;
                _debouncerAirAudioMuteMicrophone.Debounce("MuteMicrophoneCheck");
                OnPropertyChanged(nameof(MuteMicrophone_String));
            }
        }

        //Quick Pause ToggleSwitch

        private string _isQuickPause_String = Strings.On;

        public string QuickPause_String
        {
            get => _isQuickPauseStatus ? Strings.On : Strings.Off;
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
                _isQuickPauseStatus = value;
                if (value == false)
                    deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 0;
                else
                    deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 2;

                //_debouncerAirAudioQuickPause.Debounce("QuickPauseCheck");
                _debouncerAirAudio.Debounce("QuickPauseCheck");
                OnPropertyChanged(nameof(QuickPause_String));
                OnPropertyChanged(nameof(QuickPauseStatus));
            }
        }

        //AnswerCalls ToggleSwitch
        private string _isAnswerCalls_String = Strings.On;

        public string AnswerCalls_String
        {
            get => _isAnswerCallsStatus ? Strings.On : Strings.Off;
        }

        private bool _supportedAnswerCalls = true;

        public bool SupportedAnswerCalls
        {
            get
            {
                return _supportedAnswerCalls;
            }
            set
            {
                _supportedAnswerCalls = value;
                OnPropertyChanged(nameof(SupportedAnswerCalls));
            }
        }

        private bool _isAnswerCallsStatus = false;

        public bool AnswerCallsStatus
        {
            get
            {
                _isAnswerCallsStatus = deviceInfoDTP.AnswerCall;
                return _isAnswerCallsStatus;
            }
            set
            {
                _isRestoreEnable = false;
                _isAnswerCallsStatus = value;
                deviceInfoDTP.AnswerCall = value;
                _supportedAnswerCalls = false;
                if (isAirAudio == false)
                {
                    _deviceManager.SetBoomMicAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.AnswerCall).Wait();
                }
                _supportedAnswerCalls = true;
                //_debouncerAirAudio.Debounce("AnswerCallsCheck");
                OnPropertyChanged(nameof(AnswerCalls_String));
                _log.Info($"[AirAudioViewModel] SetBoomMicAsync ... AnswerCallsCheck .... {deviceInfoDTP.AnswerCall.ToString()}");
            }
        }

        #endregion AirAudioAutomatedActions ToggleSwitch Binding

        #region AirAudioAutomatedActions Grid Show/Hide

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

        private bool _automatedActionsWhenAirAudioIsRemovedPageShow = false;

        public bool AutomatedActionsWhenAirAudioIsRemovedPageShow
        {
            get => _automatedActionsWhenAirAudioIsRemovedPageShow;
            set
            {
                _automatedActionsWhenAirAudioIsRemovedPageShow = value;
                OnPropertyChanged(nameof(AutomatedActionsWhenAirAudioIsRemovedPageShow));
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

        #endregion AirAudioAutomatedActions Grid Show/Hide

        #region AirAudioAutomatedActionsRightView

        private bool _isLowChecked;
        private bool _isNormal2Checked;

        public bool IsNormal2Checked
        {
            get => _isNormal2Checked;
            set
            {
                if (_isNormal2Checked == value && value == true)
                {
                    return;
                }

                if (_isNormal2Checked != value && value)
                {
                    _isNormal2Checked = value;
                    if (value)
                    {
                        _isLowChecked = false;
                        deviceInfoDTP.WearDetectionSensitivityFromDTP = Convert.ToInt32(value);
                        _debouncerAirAudio.Debounce("Normal2Check");
                        OnPropertyChanged(nameof(IsNormal2Checked));
                        OnPropertyChanged(nameof(IsLowChecked));
                    }
                }
            }
        }

        public bool IsLowChecked
        {
            get => _isLowChecked;
            set
            {
                if (_isLowChecked == value && value == true)
                {
                    return;
                }

                if (_isLowChecked != value && value)
                {
                    _isLowChecked = value;
                    if (value)
                    {
                        _isNormal2Checked = false;
                        deviceInfoDTP.WearDetectionSensitivityFromDTP = Convert.ToInt32(!value);
                        _debouncerAirAudio.Debounce("LowCheck");
                        OnPropertyChanged(nameof(IsNormal2Checked));
                        OnPropertyChanged(nameof(IsLowChecked));
                    }
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
                if (_isNormalChecked == value && value == true || _isQuickPauseStatus == false)
                {
                    return;
                }
                if (_isNormalChecked != value && value)
                {
                    _isNormalChecked = value;
                    if (value)
                    {
                        _isSensitiveChecked = false;
                        int setWear = (value == true) ? 1 : 2;
                        deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = setWear;
                        _debouncerAirAudio.Debounce("NormalCheck");
                        OnPropertyChanged(nameof(IsNormalChecked));
                        OnPropertyChanged(nameof(IsSensitiveChecked));
                    }
                }
            }
        }

        public bool IsSensitiveChecked
        {
            get => _isSensitiveChecked;
            set
            {
                if (_isSensitiveChecked == value && value == true || _isQuickPauseStatus == false)
                {
                    return;
                }

                if (_isSensitiveChecked != value && value)
                {
                    _isSensitiveChecked = value;
                    if (value)
                    {
                        _isNormalChecked = false;
                        int setWear = (value == true) ? 2 : 1;
                        deviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = setWear;
                        _debouncerAirAudio.Debounce("SensitiveCheck");
                        OnPropertyChanged(nameof(IsNormalChecked));
                        OnPropertyChanged(nameof(IsSensitiveChecked));
                    }
                }
            }
        }

        #endregion AirAudioAutomatedActionsRightView

        #region AirAudioAutomatedActionsToolTip

        private string _wearDetectionToolTip = Strings.HeadsetAutomatedActionsToolTip_1;//"Automatic actions when you remove your AirAudio";

        public string WearDetectionToolTip
        {
            get => _wearDetectionToolTip;
        }

        private string _pauseMusicToolTip = Strings.HeadsetAutomatedActionsToolTip_2;//"Pauses music automatically when AirAudio is removed. Music will resume automatically when AirAudio is put on.";

        public string PauseMusicToolTip
        {
            get => _pauseMusicToolTip;
        }

        private string _muteMicrophoneToolTip = Strings.HeadsetAutomatedActionsToolTip_3;//"Mutes microphone automatically when AirAudio is removed";

        public string MuteMicrophoneToolTip
        {
            get => _muteMicrophoneToolTip;
        }

        private string _answerCallsToolTip = Strings.HeadsetAutomatedActionsToolTip_4;//"Pull down boom mic to answer calls";

        public string AnswerCallsToolTip
        {
            get => _answerCallsToolTip;
        }

        private string _quickPauseToolTip = Strings.HeadsetAutomatedActionsToolTip_5;//"Automatic actions when you move an ear cup off your ear";

        public string QuickPauseToolTip
        {
            get => _quickPauseToolTip;
        }

        #endregion AirAudioAutomatedActionsToolTip

        /// <summary>
        /// AirAudioDeviceSettings Page
        /// </summary>

        #region AirAudioDeviceSettings ToggleSwitch Binding

        //BusyLight ToggleSwitch
        private string _isBusyLight_String = Strings.On;

        public string BusyLight_String
        {
            get => _isBusyLightStatus ? Strings.On : Strings.Off;
        }

        private bool _supportedBusyLight = true;

        public bool SupportedBusyLight
        {
            get
            {
                return _supportedBusyLight;
            }
            set
            {
                _supportedBusyLight = value;
                OnPropertyChanged("SupportedBusyLight");
            }
        }

        private bool _isBusyLightStatus = false;

        public bool BusyLightStatus
        {
            get
            {
                _isBusyLightStatus = deviceInfoDTP.BusyLight;//_deviceManager.GetBusyLightAsync(CurrentDeviceInfo!.ID.ToString()).Result; //CurrentDeviceInfo!.BusyLight;
                return _isBusyLightStatus;
            }
            set
            {
                _isRestoreEnable = false;
                _isBusyLightStatus = value;
                deviceInfoDTP.BusyLight = value;
                _supportedBusyLight = false;
                if (IsDTPReady)
                {
                    _deviceManager.SetAirAudioBusyLightAsync(CurrentDeviceInfo!.ID.ToString(), deviceInfoDTP.BusyLight).Wait();
                }
                else
                    _deviceManager.SetBusyLight(value, CurrentDeviceInfo!.ID).Wait();
                _supportedBusyLight = true;
                //_debouncerAirAudio.Debounce("BusyLightCheck");
                OnPropertyChanged("BusyLight_String");
                _log.Info($"[AirAudioViewModel] SetBusyLightAsync ....... {deviceInfoDTP.BusyLight.ToString()}");
            }
        }

        #endregion AirAudioDeviceSettings ToggleSwitch Binding

        #region AirAudioDeviceSettings Grid Show/Hide

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

        #endregion AirAudioDeviceSettings Grid Show/Hide

        #region AirAudioDeviceSettingsRightView

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
                if (_isEssentialChecked == value && value == true)
                {
                    return;
                }

                if (_isEssentialChecked != value && value)
                {
                    _isEssentialChecked = value;
                    if (_isEssentialChecked)
                    {
                        _isRestoreEnable = false;
                        _isAllChecked = false;
                        deviceInfoDTP.VoiceGuidance = false;
                        if (IsDTPReady)
                            _debouncerAirAudio.Debounce("EssentialCheck");
                        else
                            _deviceManager.SetVoiceGuidance(false, CurrentDeviceInfo!.ID).Wait();
                        OnPropertyChanged(nameof(IsAllChecked));
                    }
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
                if (_isAllChecked == value && value == true)
                {
                    return;
                }

                if (_isAllChecked != value && value)
                {
                    _isAllChecked = value;
                    if (_isAllChecked)
                    {
                        _isRestoreEnable = false;
                        _isEssentialChecked = false;
                        deviceInfoDTP.VoiceGuidance = true;
                        if (IsDTPReady)
                            _debouncerAirAudio.Debounce("AllCheck");
                        else
                            _deviceManager.SetVoiceGuidance(true, CurrentDeviceInfo!.ID).Wait();
                        OnPropertyChanged(nameof(IsEssentialChecked));
                    }
                }
            }
        }

        #endregion AirAudioDeviceSettingsRightView

        #region AirAudioDeviceSettingsToolTip

        private string _busyLightToolTip = Strings.HeadsetDeviceSettingsToolTip_1;//"Indicator light when on a call";

        public string BusyLightToolTip
        {
            get => _busyLightToolTip;
        }

        private string _voiceGuidanceToolTip = Strings.HeadsetDeviceSettingsToolTip_2;//"Audio prompts and announcements for device features";

        public string VoiceGuidanceToolTip
        {
            get => _voiceGuidanceToolTip;
        }

        #endregion AirAudioDeviceSettingsToolTip

        #region lock/unlock
        private Visibility _isAncModeLocked = Visibility.Collapsed;

        public Visibility isAncModeLocked
        {
            get { return _isAncModeLocked; }
            set
            {
                _isAncModeLocked = value;
                OnPropertyChanged("isAncModeLocked");
            }
        }

        private bool _isAncEnabled = true;

        public bool isAncEnabled
        {
            get { return _isAncEnabled; }
            set
            {
                _isAncEnabled = value;
                OnPropertyChanged("isAncEnabled");
            }
        }

        private Visibility _isMicCancelLocked = Visibility.Collapsed;

        public Visibility isMicCancelLocked
        {
            get { return _isMicCancelLocked; }
            set
            {
                _isMicCancelLocked = value;
                OnPropertyChanged("isMicCancelLocked");
            }
        }

        private bool _isMicTabStopped = true;
        public bool isMicTabStopped
        {
            get { return _isMicTabStopped; }
            set
            {
                _isMicTabStopped = value;
                OnPropertyChanged("isMicTabStopped");
            }
        }

        private Visibility _isWearLocked = Visibility.Collapsed;

        public Visibility isWearLocked
        {
            get { return _isWearLocked; }
            set
            {
                _isWearLocked = value;
                OnPropertyChanged("isWearLocked");
            }
        }

        private bool _isWearTabStopped = true;
        public bool isWearTabStopped
        {
            get { return _isWearTabStopped; }
            set
            {
                _isWearTabStopped = value;
                OnPropertyChanged("isWearTabStopped");
            }
        }
        #endregion

        public int ConvertVersionToInt(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Version string cannot be null or empty.");

            string numericVersion = version.Replace(".", "");

            if (int.TryParse(numericVersion, out int result))
            {
                return result;
            }
            else
            {
                throw new FormatException("Invalid version format. Could not convert to integer.");
            }
        }

        private bool CheckIsDirty()
        {
            bool IsDirty = _deviceManager.GetAirAudioIsDirtyAsync(CurrentDeviceInfo!.ID.ToString()).Result;
            return IsDirty;
        }

    }
}