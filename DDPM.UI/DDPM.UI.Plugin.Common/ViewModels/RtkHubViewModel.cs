using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DDPM.UI.Plugin.ViewModels
{
    public class RtkHubViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables

        public readonly ILog _log;
        public IDeviceManagerSA _deviceManager;
        public DeviceInfoDTP DeviceInfoDTP;
        //private Debouncer _debouncerHeadset;
        //private Debouncer _debouncerHeadsetPauseMusic;
        //private Debouncer _debouncerHeadsetMuteMicrophone;
        //private Debouncer _debouncerHeadsetQuickPause;
        //private Debouncer _debouncerHeadsetSidetoneCheck;
        //public event EventHandler<EventArgs> HeadsetSettingChanged = delegate { };
        //public event EventHandler<EventArgs> HeadsetGroupChanged = delegate { };
        //public event EventHandler<EventArgs> BtnRestoreChanged = delegate { };
        //public bool _waitHeadsetReady_DTP;
        //public bool _waitHeadsetReady_DTH;
        #endregion Variables

        public new event PropertyChangedEventHandler? PropertyChanged;

        public RtkHubViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));
            Requires.NotNull(deviceManager, nameof(deviceManager));
            _log = log;
            _deviceManager = deviceManager;
            DeviceInfoDTP = new DeviceInfoDTP();
            DebouncerFfunctionInit();
            _log.Info($"[RtkHubViewModel] HeadsetViewModel Start ...");
        }

        public void UloadHeadset_DTPNotify()
        {
            _log.Info($"[RtkHubViewModel] UloadHeadset_DTPNotify ...");
            //HeadsetSettingChanged -= HeadsetSettingChanged;
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify -= Headset_DTPNotify;
                DdpmCommonHelper.BitmapImageUpdated -= ImageUpdate;
            }
        }

        private void DebouncerFfunctionInit()
        {
            //_debouncerHeadset = new Debouncer(1000, ExecuteDebouncedAction);
            //_debouncerHeadsetPauseMusic = new Debouncer(1000, ExecuteDebouncedActionForPauseMusic);
            //_debouncerHeadsetMuteMicrophone = new Debouncer(1000, ExecuteDebouncedActionForMuteMicrophone);
            //_debouncerHeadsetQuickPause = new Debouncer(1000, ExecuteDebouncedActionForQuickPause);
            //_debouncerHeadsetSidetoneCheck = new Debouncer(1000, ExecuteDebouncedActionForSidetoneCheck);
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
                _log.Info($"[RtkHubViewModel] deal_param exception {ex.Message.ToString()}");
            }
            return tmp;
        }

        private void Headset_DTPNotify(object? sender, UpdateUINotify e)
        {
            //Dictionary<string, string> event_param = deal_param(e.UI_Field_Name);
            //try
            //{
            //    if (event_param == null || event_param.Count == 0)
            //    {
            //        _log.Info($"[HeadsetViewModel] Headset_DTPNotify null or 0");
            //        return;
            //    }
            //    if (!event_param.TryGetValue("Device", out var device))
            //    {
            //        _log.Info($"[HeadsetViewModel] Device cannot be found in event_param");
            //        return;
            //    }
            //    if (device == "Headset")
            //    {
            //        if (!event_param.TryGetValue("EventType", out var eventtype))
            //        {
            //            _log.Info($"[HeadsetViewModel] EventType cannot be found in event_param");
            //            return;
            //        }
            //        if (!event_param.TryGetValue("DeviceId", out var guid))
            //        {
            //            _log.Info($"[HeadsetViewModel] GUID cannot be found in event_param");
            //            return;
            //        }
            //        _log.Info($"[HeadsetViewModel] EventType = {eventtype}");

            //        switch (eventtype)
            //        {
            //            case "Headset_Connected":
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_Connected {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;
            //            case "Headset_Disconnected":
            //                _waitHeadsetReady_DTP = false;
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_Disconnected {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;
            //            case "Headset_PairedHostNameChanged":
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_PairedHostNameChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;
            //            case "Headset_WearDetectionChanged":
            //                HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isWearDetectionStatus);
            //                DeviceInfoDTP.WearDetectionFromDTP = _isWearDetectionStatus;
            //                CheckWearDetectionUI();
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_WearDetectionChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;

            //            case "Headset_IsWearDetectionPauseMusicEnabledChanged":
            //                HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isPauseMusicStatus);
            //                DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = _isPauseMusicStatus;
            //                CheckWearDetectionUI();
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_IsWearDetectionPauseMusicEnabledChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;

            //            case "Headset_IsWearDetectionMuteMicEnabledChanged":
            //                HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isMuteMicrophoneStatus);
            //                DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = _isMuteMicrophoneStatus;
            //                CheckWearDetectionUI();
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_IsWearDetectionMuteMicEnabledChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;

            //            case "Headset_WearDetectionQuickPauseChanged":
            //                HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isQuickPauseStatus);
            //                DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = BoolToInt(_isQuickPauseStatus);
            //                if (Model == "WL7024")
            //                {
            //                    if (event_param[eventtype].ToString().ToLower() == "off")
            //                    {
            //                        _isQuickPauseStatus = false;
            //                        DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 0;
            //                        //_isNormalChecked = true;
            //                        //_isSensitiveChecked = false;
            //                    }
            //                    if (event_param[eventtype].ToString().ToLower() == "sensitive")
            //                    {
            //                        _isQuickPauseStatus = true;
            //                        DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 2;
            //                        _isNormalChecked = false;
            //                        _isSensitiveChecked = true;
            //                    }
            //                    if (event_param[eventtype].ToString().ToLower() == "normal")
            //                    {
            //                        if (!_isQuickPauseStatus)
            //                        {
            //                            _isQuickPauseStatus = !_isQuickPauseStatus;
            //                        }
            //                        DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 1;
            //                        _isNormalChecked = true;
            //                        _isSensitiveChecked = false;
            //                    }
            //                    _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_WearDetectionQuickPauseChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                }
            //                CheckWearDetectionUI();
            //                break;

            //            case "Headset_WearDetectionSensitivityChanged":
            //                if (Model == "WL5024")
            //                {
            //                    if (event_param[eventtype].ToString().ToLower() == "normal")
            //                    {
            //                        _isNormal2Checked = true;
            //                        _isLowChecked = false;
            //                    }
            //                    if (event_param[eventtype].ToString().ToLower() == "low")
            //                    {
            //                        _isNormal2Checked = false;
            //                        _isLowChecked = true;
            //                    }
            //                    _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_WearDetectionSensitivityChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                }
            //                if (Model == "WL7024")
            //                {
            //                    HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isQuickPauseStatus);
            //                    DeviceInfoDTP.WearDetectionSensitivityFromDTP = BoolToInt(_isQuickPauseStatus);
            //                    _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_WearDetectionSensitivityChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                }
            //                CheckWearDetectionUI();
            //                break;
            //            case "Headset_BandsGainChanged":
            //                // DTH event still support, DTP keep empty.
            //                break;
            //            case "Headset_BoomMicChanged":
            //                DeviceInfoDTP.AnswerCall = event_param[eventtype].ToLower() == "true" ? true : false;
            //                CheckAnswerCallUI(true);
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_BoomMicChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;
            //            case "Headset_BoomMicSupportedChangedArgs":
            //                DeviceInfoDTP.IsAnswerCallSupported = event_param[eventtype].ToLower() == "true" ? true : false;
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_BoomMicSupportedChangedArgs {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;
            //            case "Headset_FirmwareVersionChanged":
            //                FirmwareVersion2 = event_param[eventtype];
            //                FirmwareVersion2 = string.Join(".", FirmwareVersion2.ToCharArray());
            //                FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify ... Headset_FirmwareVersionChanged ... {FirmwareVersion2} ...");
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_FirmwareVersionChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;
            //            case "Headset_IsReadyChanged":
            //                if (!_waitHeadsetReady_DTP)
            //                {
            //                    _waitHeadsetReady_DTP = true;
            //                    //FirmwareVersion2 = _deviceManager.GetHeadsetFirmwareVersionAsync(CurrentDeviceID.ToString()).Result;
            //                    //FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";
            //                }
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_IsReadyChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;
            //            case "Headset_IsDirtyChanged":
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_IsDirtyChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;
            //            case "Headset_SetFactoryResetAsyncValueForHeadset":
            //                if (CheckIfCurrentSettingsMatchDefault(DeviceInfoDTP, Model))
            //                    return;
            //                if (guid != CurrentDeviceInfo?.ID.ToString())
            //                    RestoreToDefault(false);
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_SetFactoryResetAsyncValueForHeadset {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;
            //            case "Headset_SetFactoryResetAsyncValueForHeadsetForCLI":
            //                RestoreToDefault(false);// For CLI Update
            //                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_SetFactoryResetAsyncValueForHeadset {Model.ToString() + " : " + event_param[eventtype].ToString()}");
            //                break;
            //            default:
            //                break;
            //        }
            //        UpdateResetToDefault();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _log.Error($"[HeadsetViewModel] Headset_DTPNotify Exception = {ex.Message.ToString()}");
            //}
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
                _log.Error($"[RtkHubViewModel] HandleWearDetectionEvent Exception = {ex.Message.ToString()}");
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
            //_isRestoreEnable = false;
            //if (param is string mode)
            //{
            //    switch (mode)
            //    {
            //        // Button類
            //        case "ANC":
            //        case "Transparency":
            //        case "NoiseOff":
            //            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetAncModeAsync ... {mode} ... {DeviceInfoDTP.AncMode.ToString()}");
            //            _deviceManager.SetAncModeAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.AncMode).Wait();
            //            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetSidetoneAsync ... SidetoneCheck .... {DeviceInfoDTP.Sidetone.ToString()}");
            //            _deviceManager.SetSidetoneAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.Sidetone).Wait();
            //            break;
            //        case "TransparencylevelSlider":
            //            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetAncGainAsync ... Transparencylevel ... {DeviceInfoDTP.AncGain.ToString()}");
            //            _deviceManager.SetAncGainAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.AncGain).Wait();
            //            break;
            //        //------------------------------------------------------------------------------------------
            //        // Button類
            //        case "DefaultCheck":
            //        case "BassBoostCheck":
            //        case "SpeechBoostCheck":
            //        case "TrebleBoostCheck":
            //        case "CustomCheck":
            //            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetSelectedPresetAsync ... {mode} ... {DeviceInfoDTP.SelectedPreset.ToString()}");
            //            _deviceManager.SetSelectedPresetAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.SelectedPreset).Wait();
            //            break;
            //        //------------------------------------------------------------------------------------------
            //        case "WearDetectionCheck":
            //            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetWearDetectionAsync ... WearDetectionCheck ... {DeviceInfoDTP.WearDetectionFromDTP.ToString()}");
            //            _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.WearDetectionFromDTP).Wait();
            //            break;
            //        case "PauseMusicCheck":
            //        //_log.Info($"[HeadsetViewModel] SetIsWearDetectionPauseMusicEnabledAsync ... PauseMusicCheck ... {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()} ...");
            //        //_deviceManager.SetIsWearDetectionPauseMusicEnabledAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP).Wait();
            //        //break;
            //        case "MuteMicrophoneCheck":
            //        //_log.Info($"[HeadsetViewModel] SetIsWearDetectionMuteMicEnabledAsync ... MuteMicrophoneCheck ... {DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP.ToString()}");
            //        //_deviceManager.SetIsWearDetectionMuteMicEnabledAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP).Wait();
            //        //break;
            //        // Button類
            //        case "QuickPauseCheck":
            //        case "NormalCheck":
            //        case "SensitiveCheck":
            //            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetWearDetectionQuickPauseAsync ... {mode} ... {DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP.ToString()}");
            //            _deviceManager.SetWearDetectionQuickPauseAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP).Wait();
            //            break;
            //        //------------------------------------------------------------------------------------------
            //        // Button類
            //        case "Normal2Check":
            //        case "LowCheck":
            //            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetWearDetectionSensitivityAsync ... {mode} ... {DeviceInfoDTP.WearDetectionSensitivityFromDTP.ToString()}");
            //            _deviceManager.SetWearDetectionSensitivityAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.WearDetectionSensitivityFromDTP).Wait();
            //            break;
            //        //------------------------------------------------------------------------------------------
            //        case "AnswerCallsCheck":
            //        //_log.Info($"[HeadsetViewModel] SetBoomMicAsync ... AnswerCallsCheck .... {DeviceInfoDTP.AnswerCall.ToString()}");
            //        //_deviceManager.SetBoomMicAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.AnswerCall).Wait();
            //        //break;
            //        //------------------------------------------------------------------------------------------
            //        case "BusyLightCheck":
            //            //_log.Info($"[HeadsetViewModel] SetBusyLightAsync ....... {DeviceInfoDTP.BusyLight.ToString()}");
            //            //_deviceManager.SetBusyLightAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.BusyLight).Wait();
            //            break;
            //        //------------------------------------------------------------------------------------------
            //        // Button類
            //        case "EssentialCheck":
            //        case "AllCheck":
            //            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetVoiceGuidanceAsync ... {mode} .... {DeviceInfoDTP.BusyLight.ToString()}");
            //            _deviceManager.SetVoiceGuidanceAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.VoiceGuidance).Wait();
            //            break;
            //        //------------------------------------------------------------------------------------------
            //        case "OutgoingAudioCheck":
            //        //_log.Info($"[HeadsetViewModel] SetMicNoiseCancellationAsync ... OutgoingAudioCheck .... {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
            //        //_deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.MicNoiseCancellation).Wait();
            //        //break;
            //        case "IncomingAudioCheck":
            //        //_log.Info($"[HeadsetViewModel] SetMicNCIncomingAsync ... IncomingAudioCheck .... {DeviceInfoDTP.MicNCIncoming.ToString()}");
            //        //_deviceManager.SetMicNCIncomingAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.MicNCIncoming).Wait();
            //        //break;
            //        case "MicNoiseCancellationCheck":
            //        //_log.Info($"[HeadsetViewModel] SetMicNoiseCancellationAsync ... MicNoiseCancellationCheck .... {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
            //        //_deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.MicNoiseCancellation).Wait();
            //        //break;
            //        case "SidetoneCheck":
            //            //_log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetSidetoneAsync ... SidetoneCheck .... {DeviceInfoDTP.Sidetone.ToString()}");
            //            //_deviceManager.SetSidetoneAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.Sidetone).Wait();
            //            break;
            //        case "SidetoneSlider":
            //            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SidetoneLevel ... SidetoneLevel .... {DeviceInfoDTP.SidetoneLevel.ToString()}");
            //            _deviceManager.SetSidetoneLevelAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.SidetoneLevel).Wait();
            //            break;
            //        //------------------------------------------------------------------------------------------
            //        default:
            //            break;
            //    }
            //}
        }

        private void ExecuteDebouncedActionForPauseMusic(object param)
        {
            _isUpdateEnable = false;
            _log.Info($"[HeadsetViewModel] ExecuteDebounced SetIsWearDetectionPauseMusicEnabledAsync ... PauseMusicCheck ... {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()} ...");
            _deviceManager.SetIsWearDetectionPauseMusicEnabledAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP).Wait();
        }

        private void ExecuteDebouncedActionForMuteMicrophone(object param)
        {
            _isUpdateEnable = false;
            _log.Info($"[HeadsetViewModel] ExecuteDebounced SetIsWearDetectionMuteMicEnabledAsync ... MuteMicrophoneCheck ... {DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP.ToString()}");
            _deviceManager.SetIsWearDetectionMuteMicEnabledAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP).Wait();
        }
        private void ExecuteDebouncedActionForQuickPause(object param)
        {
            _isUpdateEnable = false;
            _log.Info($"[HeadsetViewModel] ExecuteDebounced SetWearDetectionQuickPauseAsync ... QuickPauseCheck ... {DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP.ToString()}");
            _deviceManager.SetWearDetectionQuickPauseAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP).Wait();
        }
        private void ExecuteDebouncedActionForSidetoneCheck(object param)
        {
            _isUpdateEnable = false;
            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetSidetoneAsync ... SidetoneCheck .... {DeviceInfoDTP.Sidetone.ToString()}");
            _deviceManager.SetSidetoneAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.Sidetone).Wait();

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
            //_log.Info($"[HeadsetViewModel] DetectPageShow ... {model}");
            //ReadQRCodeReg();
            //AllResetHeadsetPage();
            //switch (model.ToUpper())
            //{
            //    case "WL7024"://Mito
            //        //Page 1
            //        _controlTheNoiseIHearPageShow = true;
            //        _configureMyAudioModesPageShow = true;
            //        //_sidetonePageShow = false;
            //        //Page 2
            //        _wearDetectionPageShow = true;
            //        _automatedActionsWhenHeadsetIsRemovedPageShow = true;
            //        _automatedActionsQuickPausePageShow = true;
            //        _automatedActionsSensitivityPageShow = true;
            //        //Page 3
            //        _voiceGuidancePageShow = true;
            //        //_deviceSettingsDownloadDellAudioPageShow = false;
            //        break;

            //    case "WL5024"://Pegasus
            //        //Page 1
            //        _controlTheNoiseIHearPageShow = true;
            //        _configureMyAudioModesPageShow = true;
            //        //Page 2
            //        _wearDetectionPageShow = true;
            //        _automatedActionsSensitivityUpPageShow = true;
            //        _automatedActionsWhenHeadsetIsRemovedPageShow = true;
            //        _automatedActionsAnswerCallPageShow = DeviceInfoDTP.IsAnswerCallSupported; //true; //WL5024 page2, not only AnswerCall  // always show, but need to detect disable/enable
            //        _supportedAnswerCalls = true; //WL5024 page2, always need to show
            //        //SupportedAnswerCalls = true;
            //        //Page 3
            //        _voiceGuidancePageShow = true;
            //        //_deviceSettingsDownloadDellAudioPageShow = false;
            //        break;

            //    case "WH5024"://Winflo
            //        //Page 1
            //        _controlTheNoiseIHearPageShow = true;
            //        _configureMyAudioModesPageShow = true;
            //        //Page 2                  
            //        _automatedActionsAnswerCallPageShow = true;//DELL 說拿掉;// only AnswerCall // always show, but need to detect disable/enable
            //        _supportedAnswerCalls = DeviceInfoDTP.IsAnswerCallSupported;
            //        //SupportedAnswerCalls = DeviceInfoDTP.IsAnswerCallSupported;
            //        //Page 3
            //        _voiceGuidancePageShow = true;
            //        _deviceSettingsDownloadDellAudioPageShow = false;
            //        break;

            //    case "WL3024"://Vaporify
            //        //Page 1
            //        _configureMyAudioModesPageShow = true;
            //        //Page 2                   
            //        _automatedActionsAnswerCallPageShow = true;
            //        _supportedAnswerCalls = DeviceInfoDTP.IsAnswerCallSupported;
            //        //SupportedAnswerCalls = DeviceInfoDTP.IsAnswerCallSupported;
            //        //Page 3
            //        _voiceGuidancePageShow = true;
            //        //_deviceSettingsDownloadDellAudioPageShow = false;
            //        break;

            //    case "WH3024"://Airmax
            //        //Page 1
            //        _controlTheNoiseIHearPageShow = false;//Fix PIMS-PIMS-294568
            //        _configureMyAudioModesPageShow = true;
            //        //Page 2
            //        _supportedAnswerCalls = DeviceInfoDTP.IsAnswerCallSupported;
            //        //SupportedAnswerCalls = DeviceInfoDTP.IsAnswerCallSupported;
            //        _automatedActionsAnswerCallPageShow = true;
            //        //Page 3
            //        _deviceSettingsDownloadDellAudioPageShow = false;
            //        //defult page
            //        break;

            //    default:
            //        break;
            //}
            //CheckHeadsetFunc();
        }
        public void ReadQRCodeReg()
        {
            //object regValue = null;
            //try
            //{
            //    if (DdpmCommonHelper.DeviceManagerSA != null)
            //    {
            //        //regValue = DdpmCommonHelper.DeviceManagerSA!.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, RegPath, RegKeyForQRCode).Result;
            //        regValue = DdpmCommonHelper.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, RegPath, RegKeyForQRCode);

            //        if (regValue != null)
            //        {

            //            if (Convert.ToBoolean(regValue))
            //            {
            //                _deviceSettingsDownloadDellAudioPageShow = false;
            //                _log.Info($"[HeadsetViewModel] ReadQRCodeReg ....... success true");
            //            }
            //            else
            //            {
            //                _deviceSettingsDownloadDellAudioPageShow = true;
            //                _log.Info($"[HeadsetViewModel] ReadQRCodeReg ....... success false");
            //            }

            //        }
            //        else
            //        {
            //            _deviceSettingsDownloadDellAudioPageShow = true;
            //            _log.Info($"[HeadsetViewModel] ReadQRCodeReg ReadRegistryData ....... fail");
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _log.Info($"[HeadsetViewModel] ReadQRCodeReg ....... {ex.ToString()}");
            //}
        }


        public void AllResetHeadsetPage()
        {
            //_log.Info($"[HeadsetViewModel] AllResetHeadsetPage ...");
            //_controlTheNoiseIHearPageShow = false;
            //_configureMyAudioModesPageShow = false;
            //_wearDetectionPageShow = false;
            //_automatedActionsWhenHeadsetIsRemovedPageShow = false;
            //_automatedActionsQuickPausePageShow = false;
            //_automatedActionsSensitivityPageShow = false;
            //_voiceGuidancePageShow = false;
            ////_deviceSettingsDownloadDellAudioPageShow = false;
            //_automatedActionsSensitivityUpPageShow = false;
            //_automatedActionsAnswerCallPageShow = false;
        }

        public void CheckHeadsetFunc()
        {
            //_log.Info($"[HeadsetViewModel] CheckHeadsetFunc ...");
            //CheckSidetoneUI(true);
            //CheckBusyLightUI(true);
            //CheckOutgoingAudioUI(true);
            //CheckMicNCIncomingUI(true);
            //CheckMicNoiseCancellationUI(true);
            //CheckWearDetectionUI(true);
            //CheckPresetsUI(true);
            //CheckVoiceGuidanceUI(true);
            //CheckANCUI(true);
            //CheckAnswerCallUI(true);
            //HeadsetSettingChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CheckSidetoneUI(bool PropertyChange)
        {
            //if (DeviceInfoDTP.IsSidetoneSupported)
            //{
            //    _isSidetoneStatus = DeviceInfoDTP.Sidetone;//_deviceManager.GetSidetoneAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo.Sidetone;

            //    if (PropertyChange)
            //    {
            //        OnPropertyChanged(nameof(SidetoneStatus));
            //        OnPropertyChanged(nameof(Sidetone_String));
            //        OnPropertyChanged(nameof(SidetoneSliderStatus));
            //        UpdateCollaborationAndultimediaUI(true, false);
            //    }
            //}
        }

        public void UpdateResetToDefault()
        {
            //if (CheckIfCurrentSettingsMatchDefault(DeviceInfoDTP, Model))
            //    _isRestoreEnable = true;
            //else
            //    _isRestoreEnable = false;
            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //{
            //    BtnRestoreChanged?.Invoke(this, EventArgs.Empty);
            //});

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
                if (deviceInfo.LogicalDeviceType.Contains("25") || deviceInfo.LogicalDeviceType.Contains("LogicalHub", StringComparison.OrdinalIgnoreCase)) //RtkHub
                    DeviceInfos.Add(deviceInfo.ID, deviceInfo);
            }
        }

        public override bool SetCurrentDevice(string instanceIDs)
        {
            _log.Info($"[RtkHubViewModel] SetCurrentDevice ... instanceIDs : {instanceIDs}");
            if (!base.SetCurrentDevice(instanceIDs))
                return false;
            //_waitHeadsetReady_DTP = false; // Before enter, make sure to reset the flag
            //_waitHeadsetReady_DTH = false; // Before enter, make sure to reset the flag
            DeviceInfoDTP = new DeviceInfoDTP();
            FirmwareVersion = Strings.FirmwareVersion + $" {FirmwareVersion}";
            //DdpmCommonHelper.DeviceManagerSA!.UIUpdateNotify += Headset_DTPNotify;
            //DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;
            _log.Info($"[RtkHubViewModel] SetCurrentDevice GUID ... {CurrentDeviceID.ToString()}");
            return true;
        }



        public override void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            //_log.Info($"[HeadsetViewModel] HandleNotification ... Receive {property.ToString()}");
            //base.HandleNotification(changeType, di, property);
            ////CheckHeadsetFunc();
            //switch (property)
            //{
            //    case "IsReadyChanged":
            //        if (!_waitHeadsetReady_DTH)
            //        {
            //            _waitHeadsetReady_DTH = true;
            //            //FirmwareVersion2 = _deviceManager.GetHeadsetFirmwareVersionAsync(CurrentDeviceID.ToString()).Result;
            //            //FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";
            //        }
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event IsReadyChanged {Model.ToString() + " : " + di.IsReady.ToString()}");
            //        break;
            //    case "IsDirtyChanged":
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event IsDirtyChanged {Model.ToString() + " : " + di.IsDirty.ToString()}");
            //        break;
            //    case "MicNoiseCancellationChanged":
            //        DeviceInfoDTP.MicNoiseCancellation = di.MicNoiseCancellation;//_deviceManager.GetMicNoiseCancellationAsync(CurrentDeviceID.ToString()).Result;
            //        CheckMicNoiseCancellationUI(true);
            //        CheckOutgoingAudioUI(true);
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event MicNoiseCancellationChanged {Model.ToString() + " : " + di.MicNoiseCancellation.ToString()}");
            //        break;

            //    case "MicNCIncomingChanged":
            //        DeviceInfoDTP.MicNCIncoming = di.MicNCIncoming;//_deviceManager.GetMicNCIncomingAsync(CurrentDeviceID.ToString()).Result;
            //        CheckMicNCIncomingUI(true);
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event MicNCIncomingChanged {Model.ToString() + " : " + di.MicNCIncoming.ToString()}");
            //        break;

            //    case "SidetoneChanged":
            //        DeviceInfoDTP.Sidetone = di.Sidetone;//_deviceManager.GetSidetoneAsync(CurrentDeviceID.ToString()).Result;
            //        CheckSidetoneUI(true);
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event SidetoneChanged {Model.ToString() + " : " + di.Sidetone.ToString()}");
            //        break;

            //    case "BusyLightChanged":
            //        DeviceInfoDTP.BusyLight = di.BusyLight;//_deviceManager.GetBusyLightAsync(CurrentDeviceID.ToString()).Result;
            //        CheckBusyLightUI(true);
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event BusyLightChanged {Model.ToString() + " : " + di.BusyLight.ToString()}");
            //        break;

            //    case "VoiceGuidanceChanged":
            //        DeviceInfoDTP.VoiceGuidance = di.VoiceGuidance;//_deviceManager.GetVoiceGuidanceAsync(CurrentDeviceID.ToString()).Result;
            //        CheckVoiceGuidanceUI(true);
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event VoiceGuidanceChanged {Model.ToString() + " : " + di.VoiceGuidance.ToString()}");
            //        break;

            //    case "SelectedPresetChanged":
            //        DeviceInfoDTP.SelectedPreset = di.SelectedPreset;//_deviceManager.GetSelectedPresetAsync(CurrentDeviceID.ToString()).Result;
            //        CheckPresetsUI(true);
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event SelectedPresetChanged {Model.ToString() + " : " + di.SelectedPreset.ToString()}");
            //        break;

            //    case "SidetoneLevelChanged":
            //        DeviceInfoDTP.SidetoneLevel = di.SidetoneLevel;//_deviceManager.GetSidetoneLevelAsync(CurrentDeviceID.ToString()).Result;
            //        CheckSidetoneLevelUI(true);
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event SidetoneLevelChanged {Model.ToString() + " : " + di.SidetoneLevel.ToString()}");
            //        break;
            //    //case "MuteStatusChanged":
            //    //    break;
            //    case "BandsGainChanged":
            //        DeviceInfoDTP.Band1Gain = di.Band1Gain;
            //        DeviceInfoDTP.Band2Gain = di.Band2Gain;
            //        DeviceInfoDTP.Band3Gain = di.Band3Gain;
            //        DeviceInfoDTP.Band4Gain = di.Band4Gain;
            //        DeviceInfoDTP.Band5Gain = di.Band5Gain;
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event BandsGainChanged {Model.ToString() + " : " + "Band1Gain = " + di.Band1Gain.ToString()}"
            //                                                                                                               + ", Band2Gain = " + di.Band2Gain.ToString()
            //                                                                                                               + ", Band3Gain = " + di.Band3Gain.ToString()
            //                                                                                                               + ", Band4Gain = " + di.Band4Gain.ToString()
            //                                                                                                               + ", Band5Gain = " + di.Band5Gain.ToString());
            //        HeadsetSettingChanged?.Invoke(this, EventArgs.Empty);
            //        break;

            //    case "AncModeChanged":
            //        DeviceInfoDTP.AncMode = di.AncMode;//_deviceManager.GetAncModeAsync(CurrentDeviceID.ToString()).Result;
            //        CheckANCUI(true);
            //        // PIMS-333300
            //        switch (DeviceInfoDTP.AncMode)
            //        {
            //            case 0:
            //            case 1:
            //                if (_wasSidetoneActiveBeforeTransparency)
            //                {
            //                    DeviceInfoDTP.Sidetone = true;
            //                    _isSidetoneStatus = true;
            //                }
            //                else
            //                {
            //                    DeviceInfoDTP.Sidetone = false;
            //                    _isSidetoneStatus = false;
            //                }
            //                //DeviceInfoDTP.Sidetone = true;
            //                //_isSidetoneStatus = true;
            //                break;

            //            case 2:
            //                DeviceInfoDTP.Sidetone = false;
            //                _isSidetoneStatus = false;
            //                break;
            //        }
            //        if (IsDTPReady)
            //        {
            //            _deviceManager.SetSidetoneAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.Sidetone).Wait();
            //            _log.Info($"[HeadsetViewModel] HandleNotification DTH Event, DTP SetSidetoneAsync {Model.ToString() + " : " + di.AncMode.ToString()}");
            //        }
            //        else
            //        {
            //            if (CurrentDeviceInfo?.ID is Guid id && !string.IsNullOrWhiteSpace(id.ToString()))
            //            {
            //                _deviceManager.SetSidetone(true, CurrentDeviceInfo.ID).Wait();
            //                _log.Info($"[HeadsetViewModel] HandleNotification DTH Event, DTH SetSidetone {Model.ToString() + " : " + di.AncMode.ToString()}");
            //            }
            //        }
            //        CheckSidetoneUI(true);
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event AncModeChanged {Model.ToString() + " : " + di.AncMode.ToString()}");
            //        break;

            //    case "AncGainChanged":
            //        DeviceInfoDTP.AncGain = di.AncGain;
            //        _isTransparencylevelSliderValue = di.AncGain;
            //        OnPropertyChanged(nameof(TransparencylevelSliderValue));
            //        _log.Info($"[HeadsetViewModel] HandleNotification DTH Event AncGainChanged {Model.ToString() + " : " + di.AncGain.ToString()}");
            //        break;
            //    default:
            //        break;
            //}
            //UpdateResetToDefault();
            //switch (changeType)
            //{
            //    case DeviceChangedType.Peripherals_SettingsChange:
            //        if (DeviceInfos.ContainsKey(di.ID))
            //        {
            //            DeviceInfos.Remove(di.ID);
            //            DeviceInfos.Add(di.ID, di);
            //            _log.Info($"[HeadsetViewModel] HandleNotification DTH Event Peripherals_SettingsChange {Model.ToString() + " : " + di.ID.ToString()}");
            //        }
            //        else
            //        {
            //            return;
            //        }
            //        if (di.ID == CurrentDeviceID)
            //        {
            //            CurrentDeviceInfo = DeviceInfos[CurrentDeviceID];
            //            switch (property)
            //            {
            //                default:
            //                    break;
            //            }
            //            //GenerateInfo();
            //        }
            //        break;

            //    default:
            //        break;
            //}
        }
        public async void RestoreToDefault(bool set = true)
        {
            //try
            //{
            //    if (IsDTPReady)
            //    {
            //        _log.Info($"[HeadsetViewModel] Print before property ...RestoreToDefault ... in");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band1Gain ...........= {DeviceInfoDTP.Band1Gain.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band2Gain ...........= {DeviceInfoDTP.Band2Gain.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band3Gain ...........= {DeviceInfoDTP.Band3Gain.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band4Gain ...........= {DeviceInfoDTP.Band4Gain.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band5Gain ...........= {DeviceInfoDTP.Band5Gain.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.AncGain .............= {DeviceInfoDTP.AncGain.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.AncMode .............= {DeviceInfoDTP.AncMode.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.BatteryLevel ........= {DeviceInfoDTP.BatteryLevel.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.BusyLight ...........= {DeviceInfoDTP.BusyLight.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.MicNoiseCancellation = {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.MicNCIncoming .......= {DeviceInfoDTP.MicNCIncoming.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Sidetone ............= {DeviceInfoDTP.Sidetone.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.SidetoneLevel .......= {DeviceInfoDTP.SidetoneLevel.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.SelectedPreset ......= {DeviceInfoDTP.SelectedPreset.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.VoiceGuidance .......= {DeviceInfoDTP.VoiceGuidance.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.WearDetection .......= {DeviceInfoDTP.WearDetectionFromDTP.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.AnswerCall ..........= {DeviceInfoDTP.AnswerCall.ToString()}");
            //        if (set)
            //        {
            //            bool SetFactoryResult = _deviceManager.SetFactoryResetAsyncValueForHeadset(CurrentDeviceInfo?.ID.ToString(), true).Result;
            //            _log.Info($"[HeadsetViewModel] SetFactoryResetAsyncValueForHeadset = {SetFactoryResult.ToString()}");
            //        }
            //        else
            //            _log.Info($"[HeadsetViewModel] SetFactoryResetAsyncValueForHeadset only Update UI");
            //        UpdateDTPValue();
            //    }
            //    else
            //    {
            //        _log.Info($"[HeadsetViewModel] DTH Print before property ...RestoreToDefault ... in");
            //        SetFactoryResetForDTH();
            //        UpdateDTHValue();
            //    }
            //    CheckHeadsetFunc();
            //    UpdateResetToDefault();
            //    //HeadsetSettingChanged?.Invoke(this, EventArgs.Empty); //CheckHeadsetFunc();裡已經有執行
            //    //_showPluginManager?.ShowHomePage();
            //}
            //catch (Exception ex)
            //{
            //    _log.Error($"[HeadsetViewModel] RestoreToDefault ...... {ex.ToString()}");
            //}
        }

        private void SetFactoryResetForDTH()
        {
            //try
            //{
            //    _log.Info($"[HeadsetViewModel] DTH SetFactoryResetForDTH ......");
            //    _log.Info($"[HeadsetViewModel] DTH ***********************************************************************");

            //    if (CurrentDeviceInfo == null || !(CurrentDeviceInfo.ID is Guid id && !string.IsNullOrWhiteSpace(id.ToString())))
            //    {
            //        _log.Error($"[HeadsetViewModel] CurrentDeviceInfo is null or has an invalid ID, unable to reset settings.");
            //        return;
            //    }

            //    string currentModel = CurrentDeviceInfo.ModelNumber;

            //    if (!ModelDefaultSettings.ContainsKey(currentModel))
            //    {
            //        _log.Warning($"[HeadsetViewModel] No default settings found for model: {currentModel}");
            //        return;
            //    }

            //    _log.Info($"[HeadsetViewModel] DTH SetFactoryResetForDTH currentModel = {currentModel}");

            //    var defaultSettings = ModelDefaultSettings[currentModel];

            //    DeviceInfoDTP.AncMode = defaultSettings.AncMode;
            //    DeviceInfoDTP.AncGain = defaultSettings.AncGain;
            //    DeviceInfoDTP.BusyLight = defaultSettings.BusyLight;
            //    DeviceInfoDTP.MicNoiseCancellation = defaultSettings.MicNoiseCancellation;
            //    DeviceInfoDTP.Sidetone = defaultSettings.Sidetone;
            //    DeviceInfoDTP.SidetoneLevel = defaultSettings.SidetoneLevel;
            //    DeviceInfoDTP.VoiceGuidance = defaultSettings.VoiceGuidance;
            //    DeviceInfoDTP.SelectedPreset = defaultSettings.SelectedPreset;
            //    DeviceInfoDTP.Band1Gain = defaultSettings.Band1Gain;
            //    DeviceInfoDTP.Band2Gain = defaultSettings.Band2Gain;
            //    DeviceInfoDTP.Band3Gain = defaultSettings.Band3Gain;
            //    DeviceInfoDTP.Band4Gain = defaultSettings.Band4Gain;
            //    DeviceInfoDTP.Band5Gain = defaultSettings.Band5Gain;
            //    DeviceInfoDTP.MicNCIncoming = defaultSettings.MicNCIncoming;
            //    DeviceInfoDTP.WearDetectionFromDTP = defaultSettings.WearDetectionFromDTP;
            //    DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = defaultSettings.IsWearDetectionPauseMusicEnableFromDTP;
            //    DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = defaultSettings.IsWearDetectionMuteMicEnabledFromDTP;
            //    DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = defaultSettings.WearDetectionQuickPauseAsyncFromDTP;
            //    DeviceInfoDTP.WearDetectionSensitivityFromDTP = defaultSettings.WearDetectionSensitivityFromDTP;
            //    DeviceInfoDTP.AnswerCall = defaultSettings.AnswerCall;

            //    //DTH no WearDetection func
            //    //Read this Headset Support function
            //    if (CurrentDeviceInfo.IsANCSupported)
            //    {
            //        DeviceInfoDTP.IsANCSupported = true;
            //        _deviceManager.SetAncMode(DeviceInfoDTP.AncMode, CurrentDeviceInfo.ID).Wait();
            //        _deviceManager.SetAncGain(DeviceInfoDTP.AncGain, CurrentDeviceInfo.ID).Wait();
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.AncMode .....................= {DeviceInfoDTP.AncMode.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.AncGain .....................= {DeviceInfoDTP.AncGain.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsANCSupported = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsANCSupportedAsync .............................. NO");
            //    }

            //    //------------------------------------------------------------------------------------
            //    if (CurrentDeviceInfo.IsBusyLightSupported)
            //    {
            //        DeviceInfoDTP.IsBusyLightSupported = true;
            //        _deviceManager.SetBusyLight(DeviceInfoDTP.BusyLight, CurrentDeviceInfo.ID).Wait();
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.BusyLight ...................= {DeviceInfoDTP.BusyLight.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsBusyLightSupported = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsBusyLightSupportedAsync ........................ NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsMicNoiseCancellationSupported)
            //    {
            //        DeviceInfoDTP.IsMicNoiseCancellationSupported = true;
            //        _deviceManager.SetMicNoiseCancellation(DeviceInfoDTP.MicNoiseCancellation, CurrentDeviceInfo.ID).Wait();
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.MicNoiseCancellation .............= {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsMicNoiseCancellationSupported = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsMicNoiseCancellationSupportedAsync ............. NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsSidetoneSupported)
            //    {
            //        DeviceInfoDTP.IsSidetoneSupported = true;
            //        _deviceManager.SetSidetone(DeviceInfoDTP.Sidetone, CurrentDeviceInfo.ID).Wait();
            //        _deviceManager.SetSidetoneLevel(DeviceInfoDTP.SidetoneLevel, CurrentDeviceInfo!.ID).Wait();
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Sidetone ...................= {DeviceInfoDTP.Sidetone.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.SidetoneLevel ...................= {DeviceInfoDTP.SidetoneLevel.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsSidetoneSupported = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsSidetoneSupportedAsync ......................... NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsVoiceGuidanceSupported)
            //    {
            //        DeviceInfoDTP.IsVoiceGuidanceSupported = true;
            //        _deviceManager.SetVoiceGuidance(true, CurrentDeviceInfo.ID).Wait();
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.VoiceGuidance ..............= {DeviceInfoDTP.VoiceGuidance.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsVoiceGuidanceSupported = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsVoiceGuidanceSupportedAsync .................... NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsPresetsSupported)
            //    {
            //        DeviceInfoDTP.IsPresetsSupported = true;
            //        _deviceManager.SetSelectedPreset(DeviceInfoDTP.SelectedPreset, CurrentDeviceInfo.ID).Wait();
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.SelectedPreset .............= {DeviceInfoDTP.SelectedPreset.ToString()}");
            //        if (CurrentDeviceInfo.IsEqualizerSupported)
            //        {
            //            // DTH no Band Gain
            //            DeviceInfoDTP.IsEqualizerSupported = true;
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band1Gain ..................= {DeviceInfoDTP.Band1Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band2Gain ..................= {DeviceInfoDTP.Band2Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band3Gain ..................= {DeviceInfoDTP.Band3Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band4Gain ..................= {DeviceInfoDTP.Band4Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band5Gain ..................= {DeviceInfoDTP.Band5Gain.ToString()}");
            //        }
            //        else
            //        {
            //            DeviceInfoDTP.IsEqualizerSupported = false;
            //            _log.Info($"[HeadsetViewModel] DTH GetIsEqualizerSupportedAsync .................... NO");
            //        }
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsPresetsSupported = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsPresetsSupportedAsync ................... NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsMicNCIncomingSupported)
            //    {
            //        DeviceInfoDTP.IsMicNCIncomingSupported = true;
            //        _deviceManager.SetMicNCIncoming(DeviceInfoDTP.MicNCIncoming, CurrentDeviceInfo.ID).Wait();
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.MicNCIncoming .............= {DeviceInfoDTP.MicNCIncoming.ToString()}");
            //    }
            //    else
            //    {
            //        _log.Info($"[HeadsetViewModel] DTH GetIsMicNCIncomingSupportedAsync ............. NO");
            //    }
            //    //------------------------------------------------------------------------------------
            //}
            //catch (Exception ex)
            //{
            //    _log.Error($"[HeadsetViewModel] DTH SetFactoryResetForDTH ...... {ex.ToString()}");
            //}
        }

        /// <summary>
        /// Update DTP Headset property Value
        /// </summary>
        /// <returns></returns>
        private void UpdateDTPValue()
        {
            //try
            //{
            //    _log.Info($"[HeadsetViewModel] DTP Print before property ...UpdateDTPValue ... in");

            //    if (DeviceInfoDTP == null)
            //    {
            //        DeviceInfoDTP = new DeviceInfoDTP();
            //        _log.Info($"[HeadsetViewModel] DTP Print before property ...UpdateDTPValue new DeviceInfo...");
            //    }

            //    _log.Info($"[HeadsetViewModel] DTP Start print and read property ......");
            //    _log.Info($"[HeadsetViewModel] ***********************************************************************");

            //    //object varr = await _deviceManager.GetHeadsetDeviceItemsExAsync();
            //    bool IsAnC = _deviceManager.GetIsANCSupportedAsync(CurrentDeviceID.ToString()).Result;
            //    //Read this Headset Support function
            //    if (IsAnC)
            //    {
            //        DeviceInfoDTP.IsANCSupported = true;
            //        DeviceInfoDTP.AncMode = _deviceManager.GetAncModeAsync(CurrentDeviceID.ToString()).Result;
            //        DeviceInfoDTP.AncGain = _deviceManager.GetAncGainAsync(CurrentDeviceID.ToString()).Result;
            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.AncMode .....................= {DeviceInfoDTP.AncMode.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.AncGain .....................= {DeviceInfoDTP.AncGain.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsANCSupported = false;
            //        DeviceInfoDTP.AncMode = 0;
            //        DeviceInfoDTP.AncGain = 0;
            //        _log.Info($"[HeadsetViewModel] DTP GetIsANCSupportedAsync .............................. NO");
            //    }
            //    bool IsBusyLigh = _deviceManager.GetIsBusyLightSupportedAsync(CurrentDeviceID.ToString()).Result;
            //    //------------------------------------------------------------------------------------
            //    if (IsBusyLigh)
            //    {
            //        DeviceInfoDTP.IsBusyLightSupported = true;
            //        DeviceInfoDTP.BusyLight = _deviceManager.GetBusyLightAsync(CurrentDeviceID.ToString()).Result;
            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.BusyLight ...................= {DeviceInfoDTP.BusyLight.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsBusyLightSupported = false;
            //        DeviceInfoDTP.BusyLight = false;
            //        _log.Info($"[HeadsetViewModel] DTP GetIsBusyLightSupportedAsync ........................ NO");
            //    }
            //    //------------------------------------------------------------------------------------
            //    bool IsMicNoiseCancellationSupported = _deviceManager.GetIsMicNoiseCancellationSupportedAsync(CurrentDeviceID.ToString()).Result;
            //    if (IsMicNoiseCancellationSupported)
            //    {
            //        DeviceInfoDTP.IsMicNoiseCancellationSupported = true;
            //        DeviceInfoDTP.MicNoiseCancellation = _deviceManager.GetMicNoiseCancellationAsync(CurrentDeviceID.ToString()).Result;
            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.MicNoiseCancellation .............= {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsMicNoiseCancellationSupported = false;
            //        DeviceInfoDTP.MicNoiseCancellation = false;
            //        _log.Info($"[HeadsetViewModel] DTP GetIsMicNoiseCancellationSupportedAsync ............. NO");
            //    }
            //    //------------------------------------------------------------------------------------
            //    bool IsSidetoneSupported = _deviceManager.GetIsSidetoneSupportedAsync(CurrentDeviceID.ToString()).Result;
            //    if (IsSidetoneSupported)
            //    {
            //        DeviceInfoDTP.IsSidetoneSupported = true;
            //        DeviceInfoDTP.Sidetone = _deviceManager.GetSidetoneAsync(CurrentDeviceID.ToString()).Result;
            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.Sidetone ...................= {DeviceInfoDTP.Sidetone.ToString()}");

            //        DeviceInfoDTP.SidetoneLevel = _deviceManager.GetSidetoneLevelAsync(CurrentDeviceID.ToString()).Result;
            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.SidetoneLevel ...................= {DeviceInfoDTP.SidetoneLevel.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsSidetoneSupported = false;
            //        DeviceInfoDTP.Sidetone = false;
            //        _log.Info($"[HeadsetViewModel] DTP GetIsSidetoneSupportedAsync ......................... NO");
            //    }
            //    //------------------------------------------------------------------------------------
            //    bool IsVoiceGuidanceSupported = _deviceManager.GetIsVoiceGuidanceSupportedAsync(CurrentDeviceID.ToString()).Result;
            //    if (IsVoiceGuidanceSupported)
            //    {
            //        DeviceInfoDTP.IsVoiceGuidanceSupported = true;
            //        DeviceInfoDTP.VoiceGuidance = _deviceManager.GetVoiceGuidanceAsync(CurrentDeviceID.ToString()).Result;
            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.VoiceGuidance ..............= {DeviceInfoDTP.VoiceGuidance.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsVoiceGuidanceSupported = false;
            //        DeviceInfoDTP.VoiceGuidance = false;
            //        _log.Info($"[HeadsetViewModel] DTP GetIsVoiceGuidanceSupportedAsync .................... NO");
            //    }
            //    //------------------------------------------------------------------------------------
            //    bool IsPresetsSupported = _deviceManager.GetIsPresetsSupportedAsync(CurrentDeviceID.ToString()).Result;
            //    if (IsPresetsSupported)
            //    {
            //        DeviceInfoDTP.IsPresetsSupported = true;

            //        DeviceInfoDTP.SelectedPreset = _deviceManager.GetSelectedPresetAsync(CurrentDeviceID.ToString()).Result;

            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.SelectedPreset .............= {DeviceInfoDTP.SelectedPreset.ToString()}");
            //        bool IsEqualizerSupported = _deviceManager.GetIsEqualizerSupportedAsync(CurrentDeviceID.ToString()).Result;
            //        if (IsEqualizerSupported)
            //        {
            //            DeviceInfoDTP.IsEqualizerSupported = true;

            //            DeviceInfoDTP.Band1Gain = _deviceManager.GetBand1GainAsync(CurrentDeviceID.ToString()).Result;
            //            DeviceInfoDTP.Band2Gain = _deviceManager.GetBand2GainAsync(CurrentDeviceID.ToString()).Result;
            //            DeviceInfoDTP.Band3Gain = _deviceManager.GetBand3GainAsync(CurrentDeviceID.ToString()).Result;
            //            DeviceInfoDTP.Band4Gain = _deviceManager.GetBand4GainAsync(CurrentDeviceID.ToString()).Result;
            //            DeviceInfoDTP.Band5Gain = _deviceManager.GetBand5GainAsync(CurrentDeviceID.ToString()).Result;

            //            _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.Band1Gain ..................= {DeviceInfoDTP.Band1Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.Band2Gain ..................= {DeviceInfoDTP.Band2Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.Band3Gain ..................= {DeviceInfoDTP.Band3Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.Band4Gain ..................= {DeviceInfoDTP.Band4Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.Band5Gain ..................= {DeviceInfoDTP.Band5Gain.ToString()}");
            //        }
            //        else
            //        {
            //            DeviceInfoDTP.IsEqualizerSupported = false;
            //            _log.Info($"[HeadsetViewModel] DTP GetIsEqualizerSupportedAsync .................... NO");
            //        }
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsPresetsSupported = false;
            //        DeviceInfoDTP.IsEqualizerSupported = false;
            //        _log.Info($"[HeadsetViewModel] DTP GetIsPresetsSupportedAsync ................... NO");
            //    }
            //    //------------------------------------------------------------------------------------
            //    bool IsMicNCIncomingSupported = _deviceManager.GetIsMicNCIncomingSupportedAsync(CurrentDeviceID.ToString()).Result;
            //    if (IsMicNCIncomingSupported)
            //    {
            //        DeviceInfoDTP.IsMicNCIncomingSupported = true;

            //        DeviceInfoDTP.MicNCIncoming = _deviceManager.GetMicNCIncomingAsync(CurrentDeviceID.ToString()).Result;

            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.MicNCIncoming .............= {DeviceInfoDTP.MicNCIncoming.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsMicNCIncomingSupported = false;
            //        DeviceInfoDTP.MicNCIncoming = false;
            //        _log.Info($"[HeadsetViewModel] DTP GetIsMicNCIncomingSupportedAsync ............. NO");
            //    }
            //    //------------------------------------------------------------------------------------
            //    bool IsWearDetectionSupported = _deviceManager.GetIsWearDetectionSupportedAsync(CurrentDeviceID.ToString()).Result;
            //    if (IsWearDetectionSupported)
            //    {
            //        DeviceInfoDTP.IsWearDetectionSupported = true;
            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.GetIsWearDetectionSupportedAsync .............= {DeviceInfoDTP.IsWearDetectionSupported.ToString()}");
            //        if (DeviceInfoDTP.IsWearDetectionSupported)
            //        {
            //            DeviceInfoDTP.WearDetectionFromDTP = _deviceManager.GetWearDetectionAsync(CurrentDeviceID.ToString()).Result;
            //            _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.WearDetectionFromDTP .............= {DeviceInfoDTP.WearDetectionFromDTP.ToString()}");

            //            DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = _deviceManager.GetIsWearDetectionPauseMusicEnabledAsync(CurrentDeviceID.ToString()).Result;
            //            _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");

            //            DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = _deviceManager.GetIsWearDetectionMuteMicEnabledAsync(CurrentDeviceID.ToString()).Result;
            //            _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");

            //            DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = _deviceManager.GetWearDetectionQuickPauseAsync(CurrentDeviceID.ToString()).Result;
            //            _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");

            //            DeviceInfoDTP.WearDetectionSensitivityFromDTP = _deviceManager.GetWearDetectionSensitivityAsync(CurrentDeviceID.ToString()).Result;
            //            _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.WearDetectionSensitivityFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
            //        }
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsWearDetectionSupported = false;
            //        DeviceInfoDTP.WearDetectionFromDTP = false;
            //        _log.Info($"[HeadsetViewModel] DTP GetIsWearDetectionSupportedAsync ............. NO");
            //    }
            //    //------------------------------------------------------------------------------------
            //    bool IsBoomMicSupported = _deviceManager.GetIsBoomMicSupportedAsync(CurrentDeviceID.ToString()).Result;
            //    if (IsBoomMicSupported)
            //    {
            //        _supportedAnswerCalls = true;
            //        DeviceInfoDTP.IsAnswerCallSupported = true;
            //        DeviceInfoDTP.AnswerCall = _deviceManager.GetBoomMicAsync(CurrentDeviceID.ToString()).Result;
            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.AnswerCall .............= {DeviceInfoDTP.AnswerCall.ToString()}");
            //    }
            //    else
            //    {
            //        _supportedAnswerCalls = false;
            //        DeviceInfoDTP.IsAnswerCallSupported = false;
            //        DeviceInfoDTP.AnswerCall = false;
            //        if (Model == "WH3024" || Model == "WL3024" || Model == "WH5024")
            //        {
            //            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //            {
            //                HeadsetGroupChanged?.Invoke(this, EventArgs.Empty);
            //            });
            //        }
            //        _log.Info($"[HeadsetViewModel] DTP DeviceInfoDTP.AnswerCall ............. NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    //DeviceInfoDTP.BatteryLevel = await _deviceManager.GetHeadsetBatteryLevelAsync(CurrentDeviceID.ToString());
            //    //DeviceInfoDTP.SidetoneLevel = _deviceManager.GetSidetoneLevelAsync(CurrentDeviceID.ToString()).Result;
            //    PairedHostName1 = _deviceManager.GetHeadsetPairedHostName2Async(CurrentDeviceID.ToString()).Result;
            //    PairedHostName2 = _deviceManager.GetHeadsetPairedHostName3Async(CurrentDeviceID.ToString()).Result;
            //    UpdateResetToDefault();
            //    //OnPropertyChanged(nameof(IsRestoreEnable));
            //}
            //catch (Exception ex)
            //{
            //    _log.Error($"[HeadsetViewModel]DTP  UpdateDTPValue ...... {ex.ToString()}");
            //}
        }

        /// <summary>
        /// Update DTH Headset property Value
        /// </summary>
        /// <returns></returns>
        private void UpdateDTHValue()
        {
            //try
            //{
            //    _log.Info($"[HeadsetViewModel] DTH Print before property ...UpdateDTHValue ... in");

            //    if (!(CurrentDeviceInfo?.ID is Guid id && !string.IsNullOrWhiteSpace(id.ToString())))
            //    {
            //        _log.Error($"[HeadsetViewModel] UpdateDTHValue CurrentDeviceInfo is null or has an invalid ID, unable to reset settings.");
            //        return;
            //    }

            //    if (DeviceInfoDTP == null)
            //    {
            //        DeviceInfoDTP = new DeviceInfoDTP();
            //        _log.Info($"[HeadsetViewModel] DTH Print before property ...UpdateDTPValue new DeviceInfo...");
            //    }

            //    _log.Info($"[HeadsetViewModel] DTH Print after property ......");
            //    _log.Info($"[HeadsetViewModel] DTH ***********************************************************************");

            //    //object varr = await _deviceManager.GetHeadsetDeviceItemsExAsync();

            //    //Read this Headset Support function
            //    if (CurrentDeviceInfo.IsANCSupported)
            //    {
            //        DeviceInfoDTP.IsANCSupported = true;
            //        DeviceInfoDTP.AncMode = CurrentDeviceInfo.AncMode;
            //        DeviceInfoDTP.AncGain = CurrentDeviceInfo.AncGain;
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.AncMode .....................= {DeviceInfoDTP.AncMode.ToString()}");
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.AncGain .....................= {DeviceInfoDTP.AncGain.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsANCSupported = false;
            //        DeviceInfoDTP.AncMode = 0;
            //        DeviceInfoDTP.AncGain = 0;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsANCSupportedAsync .............................. NO");
            //    }
            //    //------------------------------------------------------------------------------------
            //    if (CurrentDeviceInfo.IsBusyLightSupported)
            //    {
            //        DeviceInfoDTP.IsBusyLightSupported = true;
            //        DeviceInfoDTP.BusyLight = CurrentDeviceInfo.BusyLight;
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.BusyLight ...................= {DeviceInfoDTP.BusyLight.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsBusyLightSupported = false;
            //        DeviceInfoDTP.BusyLight = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsBusyLightSupportedAsync ........................ NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsMicNoiseCancellationSupported)
            //    {
            //        DeviceInfoDTP.IsMicNoiseCancellationSupported = true;
            //        DeviceInfoDTP.MicNoiseCancellation = CurrentDeviceInfo.MicNoiseCancellation;
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.MicNoiseCancellation .............= {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsMicNoiseCancellationSupported = false;
            //        DeviceInfoDTP.MicNoiseCancellation = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsMicNoiseCancellationSupportedAsync ............. NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsSidetoneSupported)
            //    {
            //        DeviceInfoDTP.IsSidetoneSupported = true;
            //        DeviceInfoDTP.Sidetone = CurrentDeviceInfo.Sidetone;
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Sidetone ...................= {DeviceInfoDTP.Sidetone.ToString()}");

            //        DeviceInfoDTP.SidetoneLevel = CurrentDeviceInfo.SidetoneLevel;
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.SidetoneLevel ...................= {DeviceInfoDTP.SidetoneLevel.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsSidetoneSupported = false;
            //        DeviceInfoDTP.Sidetone = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsSidetoneSupportedAsync ......................... NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsVoiceGuidanceSupported)
            //    {
            //        DeviceInfoDTP.IsVoiceGuidanceSupported = true;
            //        DeviceInfoDTP.VoiceGuidance = CurrentDeviceInfo.VoiceGuidance;
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.VoiceGuidance ..............= {DeviceInfoDTP.VoiceGuidance.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsVoiceGuidanceSupported = false;
            //        DeviceInfoDTP.VoiceGuidance = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsVoiceGuidanceSupportedAsync .................... NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsPresetsSupported)
            //    {
            //        DeviceInfoDTP.IsPresetsSupported = true;
            //        DeviceInfoDTP.SelectedPreset = CurrentDeviceInfo.SelectedPreset;
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.SelectedPreset .............= {DeviceInfoDTP.SelectedPreset.ToString()}");
            //        if (CurrentDeviceInfo.IsEqualizerSupported)
            //        {
            //            DeviceInfoDTP.IsEqualizerSupported = true;
            //            DeviceInfoDTP.Band1Gain = CurrentDeviceInfo.Band1Gain;
            //            DeviceInfoDTP.Band2Gain = CurrentDeviceInfo.Band2Gain;
            //            DeviceInfoDTP.Band3Gain = CurrentDeviceInfo.Band3Gain;
            //            DeviceInfoDTP.Band4Gain = CurrentDeviceInfo.Band4Gain;
            //            DeviceInfoDTP.Band5Gain = CurrentDeviceInfo.Band5Gain;
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band1Gain ..................= {DeviceInfoDTP.Band1Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band2Gain ..................= {DeviceInfoDTP.Band2Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band3Gain ..................= {DeviceInfoDTP.Band3Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band4Gain ..................= {DeviceInfoDTP.Band4Gain.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band5Gain ..................= {DeviceInfoDTP.Band5Gain.ToString()}");
            //        }
            //        else
            //        {
            //            DeviceInfoDTP.IsEqualizerSupported = false;
            //            _log.Info($"[HeadsetViewModel] DTH GetIsEqualizerSupportedAsync .................... NO");
            //        }
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsPresetsSupported = false;
            //        DeviceInfoDTP.IsEqualizerSupported = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsPresetsSupportedAsync ................... NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsMicNCIncomingSupported)
            //    {
            //        DeviceInfoDTP.IsMicNCIncomingSupported = true;
            //        DeviceInfoDTP.MicNCIncoming = CurrentDeviceInfo.MicNCIncoming;
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.MicNCIncoming .............= {DeviceInfoDTP.MicNCIncoming.ToString()}");
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsMicNCIncomingSupported = false;
            //        DeviceInfoDTP.MicNCIncoming = false;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsMicNCIncomingSupportedAsync ............. NO");
            //    }
            //    //------------------------------------------------------------------------------------

            //    if (CurrentDeviceInfo.IsWearDetectionSupported)
            //    {
            //        DeviceInfoDTP.IsWearDetectionSupported = true;
            //        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.GetIsWearDetectionSupportedAsync .............= {DeviceInfoDTP.IsWearDetectionSupported.ToString()}");
            //        if (DeviceInfoDTP.IsWearDetectionSupported)
            //        {
            //            uint wearDetectionValue = (uint)CurrentDeviceInfo.WearDetection;
            //            if (GetBitValue(wearDetectionValue, 0) == 1)
            //                DeviceInfoDTP.WearDetectionFromDTP = true;
            //            else
            //                DeviceInfoDTP.WearDetectionFromDTP = false;

            //            if (GetBitValue(wearDetectionValue, 1) == 1)
            //                DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = true;
            //            else
            //                DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = false;

            //            if (GetBitValue(wearDetectionValue, 2) == 1)
            //                DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = true;
            //            else
            //                DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = false;

            //            if (GetBitValue(wearDetectionValue, 4) == 1)
            //                DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 1;
            //            else
            //                DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 0;

            //            //7024
            //            if (CurrentDeviceInfo.ModelNumber.Contains("7024"))
            //            {
            //                if (GetBitsValue(wearDetectionValue, 5) == 1)
            //                {
            //                    DeviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
            //                    DeviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
            //                }
            //                else
            //                {
            //                    DeviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
            //                    DeviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
            //                }
            //            }

            //            //5024
            //            if (CurrentDeviceInfo.ModelNumber.Contains("5024"))
            //            {
            //                if (GetBitValue(wearDetectionValue, 3) == 1)
            //                {
            //                    DeviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
            //                    DeviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
            //                }
            //                else
            //                {
            //                    DeviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
            //                    DeviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
            //                }
            //            }
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.WearDetectionFromDTP .............= {DeviceInfoDTP.WearDetectionFromDTP.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
            //            _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.WearDetectionSensitivityFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
            //        }
            //    }
            //    else
            //    {
            //        DeviceInfoDTP.IsWearDetectionSupported = false;
            //        DeviceInfoDTP.WearDetectionFromDTP = false;
            //        DeviceInfoDTP.WearDetection = 0;
            //        _log.Info($"[HeadsetViewModel] DTH GetIsWearDetectionSupportedAsync ............. NO");
            //    }
            //    //-----------------------------------------------------------------------------------------
            //    if (CurrentDeviceInfo.FirmwareVersion != "" && CurrentDeviceInfo.FirmwareVersion != string.Empty)
            //    {
            //        string resultFW = CurrentDeviceInfo.FirmwareVersion.Replace(".", "");
            //        int fwv = int.Parse(resultFW);
            //        if (Model == "WH3024" || Model == "WL3024" || Model == "WH5024")
            //        {
            //            int fwvThreshold = 0;

            //            switch (Model)
            //            {
            //                case "WH3024":
            //                    fwvThreshold = 278;
            //                    break;
            //                case "WH5024":
            //                    fwvThreshold = 227;
            //                    break;
            //                case "WL3024":
            //                    fwvThreshold = 1104;
            //                    break;
            //            }

            //            if (fwv > fwvThreshold)
            //            {
            //                _supportedAnswerCalls = true;
            //                DeviceInfoDTP.IsAnswerCallSupported = true;
            //                DeviceInfoDTP.AnswerCall = true;
            //                _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.AnswerCall .............= {DeviceInfoDTP.AnswerCall.ToString()}");
            //            }
            //            else
            //            {
            //                _supportedAnswerCalls = false;
            //                DeviceInfoDTP.IsAnswerCallSupported = false;
            //                DeviceInfoDTP.AnswerCall = false;
            //                System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //                {
            //                    HeadsetGroupChanged?.Invoke(this, EventArgs.Empty);
            //                });
            //                _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.AnswerCall ............. NO");
            //            }
            //        }
            //    }

            //    //DeviceInfoDTP.SidetoneLevel = CurrentDeviceInfo!.SidetoneLevel;

            //    UpdateResetToDefault();
            //    //OnPropertyChanged(nameof(IsRestoreEnable));
            //}
            //catch (Exception ex)
            //{
            //    _log.Error($"[HeadsetViewModel] DTH UpdateDTPValue ...... {ex.ToString()}");
            //}
        }

        public readonly string _regPath = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Global\QRCode";
        public string RegPath
        {
            get => _regPath;
        }

        private string _regKeyForQRCode = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.HeadsetQRCode.";
        public string RegKeyForQRCode
        {
            get => _regKeyForQRCode + Model;
        }
        bool IntToBool(int value) => value != 0;
        bool IntToBoolElse(int value) => value == 0;
        int BoolToInt(bool value) => value ? 1 : 0;

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
        //public void Invoke_PleaseWait(string model, HeadsetViewModel vm)
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

        private void DoWork_PleaseWait(string model, HeadsetViewModel vm)
        {
            //_log.Info($"[HeadsetViewModel] DoWork_PleaseWait .......");

            //if (!(CurrentDeviceID is Guid id && !string.IsNullOrWhiteSpace(id.ToString())))
            //{
            //    _log.Error($"[HeadsetViewModel] DoWork_PleaseWait CurrentDeviceID is null or has an invalid ID ... ");
            //    return;
            //}

            //FirmwareVersion2 = _deviceManager.GetHeadsetFirmwareVersionAsync(CurrentDeviceID.ToString()).Result;
            //IsDTPReady = _deviceManager.GetDTPProxyPluginReady().Result;
            //_log.Info($"[HeadsetViewModel] DoWork_PleaseWait ... GetDTPProxyPluginReady, IsDTPReady {IsDTPReady.ToString()} ...");

            //int tick = 0;
            //while (!_waitHeadsetReady_DTP && tick < 15)
            //{
            //    if (_deviceManager.GetIsReadyAsync(CurrentDeviceID.ToString()).Result)
            //    {
            //        _waitHeadsetReady_DTP = true;
            //        IsDTPReady = _deviceManager.GetDTPProxyPluginReady().Result; // update again
            //        _log.Info($"[HeadsetViewModel] DoWork_PleaseWait ... GetIsReadyAsync, true ... {tick} sec, success ...");
            //        break;
            //    }
            //    _log.Info($"[HeadsetViewModel] DoWork_PleaseWait ... GetIsReadyAsync, false ... {tick}");
            //    Task.Delay(1000).Wait();
            //    tick++;
            //}

            //if (_waitHeadsetReady_DTP)
            //{
            //    FirmwareVersion2 = _deviceManager.GetHeadsetFirmwareVersionAsync(CurrentDeviceID.ToString()).Result;
            //    FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";
            //    _log.Info($"[HeadsetViewModel] DoWork_PleaseWait ... Get waitHeadsetReady event True, {FirmwareVersion2} ...... ");
            //    UpdateDTPValue();
            //}
            //else
            //{
            //    tick = 0;
            //    while (!_waitHeadsetReady_DTH && tick < 5)
            //    {
            //        if (CurrentDeviceInfo.IsReady)
            //        {
            //            _waitHeadsetReady_DTH = true;
            //            FirmwareVersion2 = CurrentDeviceInfo.FirmwareVersion;
            //            FirmwareVersion2 = Strings.FirmwareVersion + $" {FirmwareVersion2}";
            //            _log.Info($"[HeadsetViewModel] DoWork_PleaseWait ... DTH_IsReady True, {FirmwareVersion2} ...... ");
            //            break;
            //        }
            //        _log.Info($"[HeadsetViewModel] DoWork_PleaseWait ... DTH_IsReady, false ... {tick}");
            //        Task.Delay(1000).Wait();
            //        tick++;
            //    }

            //    if (_waitHeadsetReady_DTH)
            //    {
            //        UpdateDTHValue();
            //    }
            //    else
            //    {
            //        _log.Info($"[HeadsetViewModel] DoWork_PleaseWait ... Can not get DTH_IsReady ...... {tick} sec, fail ...");
            //    }
            //}

            //DetectPageShow(model);
            //Task.Delay(500).Wait();
            //HidePleaseWait();
        }

        public void Invoke_PleaseWaitAsync(string model, HeadsetViewModel vm)
        {
            //vm.ShowPleaseWait();
            //try
            //{
            //    //using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            //    //{
            //    Task.Run(() => DoWork_PleaseWait(model, vm));
            //    //}
            //}
            ////catch (OperationCanceledException)
            ////{
            ////    _log.Error("[HeadsetViewModel] Invoke_PleaseWaitAsync timed out");
            ////    throw;
            ////}
            //catch (Exception ex)
            //{
            //    vm._log.Error($"[HeadsetViewModel] Invoke_PleaseWaitAsync exception: {ex.Message}");
            //    throw;
            //}
            ////finally
            ////{
            ////    vm.HidePleaseWait();
            ////}
        }

        private void RunWorkerCompleted_PleaseWait(object sender, RunWorkerCompletedEventArgs e)
        {
            //HidePleaseWait();
        }


        /// <summary>
        /// HeadsetAudioSettings Page
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

        public bool _isUpdateEnable = false;

        public bool IsUpdateEnable
        {
            get
            {
                return _isUpdateEnable;
            }
            set
            {
                _isUpdateEnable = value;
                OnPropertyChanged(nameof(IsUpdateEnable));
            }
        }


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
                _isOutgoingAudioStatus = DeviceInfoDTP.MicNoiseCancellation;
                return _isOutgoingAudioStatus;
            }
            set
            {
                _isUpdateEnable = false;
                DeviceInfoDTP.MicNoiseCancellation = value;
                _isOutgoingAudioStatus = value;
                _supportedOutgoingAudio = false;
                _deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.MicNoiseCancellation).Wait();
                _supportedOutgoingAudio = true;
                //_debouncerHeadset.Debounce("OutgoingAudioCheck");
                OnPropertyChanged(nameof(OutgoingAudio_String));
                _log.Info($"[HeadsetViewModel] SetMicNoiseCancellationAsync ... OutgoingAudioCheck .... {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
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
                _isIncomingAudioStatus = DeviceInfoDTP.MicNCIncoming;
                return _isIncomingAudioStatus;
            }
            set
            {
                _isUpdateEnable = false;
                DeviceInfoDTP.MicNCIncoming = value;
                _isIncomingAudioStatus = value;
                _supportedIncomingAudio = false;
                if (IsDTPReady)
                {
                    _deviceManager.SetMicNCIncomingAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.MicNCIncoming).Wait();
                    _log.Info($"[HeadsetViewModel] DTP SetMicNCIncomingAsync .... {DeviceInfoDTP.MicNCIncoming.ToString()}");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(CurrentDeviceInfo?.ID.ToString()))
                    {
                        _deviceManager.SetMicNCIncoming(value, CurrentDeviceInfo.ID).Wait();
                        _log.Info($"[HeadsetViewModel] DTH SetMicNCIncoming .... {DeviceInfoDTP.MicNCIncoming.ToString()}");
                    }
                    else
                    {
                        _log.Error($"[HeadsetViewModel] DTH SetMicNCIncoming .... ID is null or invalid ....");
                    }
                }
                //_debouncerHeadset.Debounce("IncomingAudioCheck");
                _supportedIncomingAudio = true;
                OnPropertyChanged(nameof(IncomingAudio_String));
                _log.Info($"[HeadsetViewModel] SetMicNCIncomingAsync ... IncomingAudioCheck .... {DeviceInfoDTP.MicNCIncoming.ToString()}");
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
                return _supportedMicNoiseCancellation = DeviceInfoDTP.IsMicNoiseCancellationSupported;
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
                _isMicNoiseCancellationStatus = DeviceInfoDTP.MicNoiseCancellation;
                return _isMicNoiseCancellationStatus;
            }
            set
            {
                _isUpdateEnable = false;
                DeviceInfoDTP.MicNoiseCancellation = value;
                _isMicNoiseCancellationStatus = value;
                _supportedMicNoiseCancellation = false;
                if (IsDTPReady)
                {
                    _deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo?.ID.ToString(), DeviceInfoDTP.MicNoiseCancellation).Wait();
                    _log.Info($"[HeadsetViewModel] DTP SetMicNoiseCancellationAsync .... {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(CurrentDeviceInfo?.ID.ToString()))
                    {
                        _deviceManager.SetMicNoiseCancellation(value, CurrentDeviceInfo.ID).Wait();
                        _log.Info($"[HeadsetViewModel] DTH SetMicNoiseCancellation .... {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
                    }
                    else
                    {
                        _log.Error($"[HeadsetViewModel] DTH SetMicNoiseCancellation .... ID is null or invalid ....");
                    }
                }
                //_debouncerHeadset.Debounce("MicNoiseCancellationCheck");
                _supportedMicNoiseCancellation = true;
                OnPropertyChanged(nameof(MicNoiseCancellation_String));
                _log.Info($"[HeadsetViewModel] SetMicNoiseCancellationAsync ... MicNoiseCancellationCheck .... {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
            }
        }

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
    }
}
