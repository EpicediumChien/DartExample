using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
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
    public class HeadsetViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables

        public readonly ILog _log;
        public IDeviceManagerSA _deviceManager;
        public IShowPluginManager _showPluginManager;
        public DeviceInfoDTP DeviceInfoDTP;
        public string _current_headset;
        private Debouncer _debouncerHeadset;
        private Debouncer _debouncerHeadsetPauseMusic;
        private Debouncer _debouncerHeadsetMuteMicrophone;
        private Debouncer _debouncerHeadsetQuickPause;
        private Debouncer _debouncerHeadsetSidetoneCheck;
        public event EventHandler<EventArgs> HeadsetSettingChanged;
        #endregion Variables

        public new event PropertyChangedEventHandler? PropertyChanged;

        public HeadsetViewModel(IShowPluginManager showPluginManager, IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;
            _showPluginManager = showPluginManager;
            DeviceInfoDTP = new DeviceInfoDTP();
            _current_headset = string.Empty;
            DdpmCommonHelper.DeviceManagerSA!.UIUpdateNotify += Headset_DTPNotify;
            DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;
            DebouncerFfunctionInit();
            _log!.Info($"[HeadsetViewModel] HeadsetViewModel Start...");
        }
        ~HeadsetViewModel()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA!.UIUpdateNotify -= Headset_DTPNotify;
                DdpmCommonHelper.BitmapImageUpdated -= ImageUpdate;
            }
        }
        private void DebouncerFfunctionInit()
        {
            _debouncerHeadset = new Debouncer(1000, ExecuteDebouncedAction);
            _debouncerHeadsetPauseMusic = new Debouncer(1000, ExecuteDebouncedActionForPauseMusic);
            _debouncerHeadsetMuteMicrophone = new Debouncer(1000, ExecuteDebouncedActionForMuteMicrophone);
            _debouncerHeadsetQuickPause = new Debouncer(1000, ExecuteDebouncedActionForQuickPause);
            _debouncerHeadsetSidetoneCheck = new Debouncer(1000, ExecuteDebouncedActionForSidetoneCheck);
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
                _log.Info($"[HeadsetViewModel] deal_param exception {ex.Message.ToString()}");
            }
            return tmp;
        }

        private void Headset_DTPNotify(object? sender, UpdateUINotify e)
        {
            Dictionary<string, string> event_param = deal_param(e.UI_Field_Name);
            try
            {
                if (event_param == null || event_param.Count == 0)
                {
                    _log.Info($"[HeadsetViewModel] Headset_DTPNotify null or 0");
                    return;
                }
                if (!event_param.TryGetValue("Device", out var device))
                {
                    _log.Info($"[HeadsetViewModel] Device cannot be found in event_param");
                    return;
                }
                if (device == "Headset")
                {
                    if (!event_param.TryGetValue("EventType", out var eventtype))
                    {
                        _log.Info($"[HeadsetViewModel] EventType cannot be found in event_param");
                        return;
                    }

                    _log.Info($"[HeadsetViewModel] EventType = {eventtype}");

                    switch (eventtype)
                    {
                        case "Headset_WearDetectionChanged":
                            HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isWearDetectionStatus);
                            DeviceInfoDTP.WearDetectionFromDTP = _isWearDetectionStatus;
                            _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_WearDetectionChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;

                        case "Headset_IsWearDetectionPauseMusicEnabledChanged":
                            HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isPauseMusicStatus);
                            DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = _isPauseMusicStatus;
                            _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_IsWearDetectionPauseMusicEnabledChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;

                        case "Headset_IsWearDetectionMuteMicEnabledChanged":
                            HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isMuteMicrophoneStatus);
                            DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = _isMuteMicrophoneStatus;
                            _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_IsWearDetectionMuteMicEnabledChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            break;

                        case "Headset_WearDetectionQuickPauseChanged":
                            HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isQuickPauseStatus);
                            DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = BoolToInt(_isQuickPauseStatus);
                            if (Model == "WL7024")
                            {
                                if (event_param[eventtype].ToString().ToLower() == "off")
                                {
                                    _isQuickPauseStatus = false;
                                    DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 0;
                                    _isNormalChecked = true;
                                    _isSensitiveChecked = false;
                                }
                                if (event_param[eventtype].ToString().ToLower() == "sensitive")
                                {
                                    _isQuickPauseStatus = true;
                                    DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 2;
                                    _isNormalChecked = false;
                                    _isSensitiveChecked = true;
                                }
                                if (event_param[eventtype].ToString().ToLower() == "normal")
                                {
                                    if (!_isQuickPauseStatus)
                                    {
                                        _isQuickPauseStatus = !_isQuickPauseStatus;
                                    }
                                    DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 1;
                                    _isNormalChecked = true;
                                    _isSensitiveChecked = false;
                                }
                                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_WearDetectionQuickPauseChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            }
                            break;

                        case "Headset_WearDetectionSensitivityChanged":
                            if (Model == "WL5024")
                            {
                                if (event_param[eventtype].ToString().ToLower() == "normal")
                                {
                                    _isNormal2Checked = true;
                                    _isLowChecked = false;
                                }
                                if (event_param[eventtype].ToString().ToLower() == "low")
                                {
                                    _isNormal2Checked = false;
                                    _isLowChecked = true;
                                }
                                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_WearDetectionSensitivityChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            }
                            if (Model == "WL7024")
                            {
                                HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isQuickPauseStatus);
                                DeviceInfoDTP.WearDetectionSensitivityFromDTP = BoolToInt(_isQuickPauseStatus);
                                _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_WearDetectionSensitivityChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                            }
                            break;
                        case "Headset_BandsGainChanged":
                            // DTH event still support, DTP keep empty.
                            break;
                        default:
                            break;
                    }
                    _isRestoreEnable = false;
                    CheckWearDetectionUI();
                }
                //Keep first, Reset event
                //if (device == "Headset_Reset")
                //{
                //    if (!event_param.TryGetValue("EventType", out var eventtype))
                //    {
                //        _log.Info($"[HeadsetViewModel] EventType cannot be found in event_param");
                //        return;
                //    }

                //    _log.Info($"[HeadsetViewModel] EventType = {eventtype}");
                //    switch (eventtype)
                //    {
                //        case "SetFactoryResetAsyncValue":
                //            HandleWearDetectionEvent(eventtype, event_param[eventtype], ref _isQuickPauseStatus);
                //            DeviceInfoDTP.WearDetectionSensitivityFromDTP = BoolToInt(_isQuickPauseStatus);
                //            _log.Info($"[HeadsetViewModel] Headset_DTPNotify Headset_WearDetectionSensitivityChanged {Model.ToString() + " : " + event_param[eventtype].ToString()}");
                //            break;
                //    }
                //}

            }
            catch (Exception ex)
            {
                _log.Error($"[HeadsetViewModel] Headset_DTPNotify Exception = {ex.Message.ToString()}");
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
                _log.Error($"[HeadsetViewModel] HandleWearDetectionEvent Exception = {ex.Message.ToString()}");
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
                        _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetAncModeAsync ... {mode} ... {DeviceInfoDTP.AncMode.ToString()}");
                        _deviceManager.SetAncModeAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.AncMode).Wait();
                        _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetSidetoneAsync ... SidetoneCheck .... {DeviceInfoDTP.Sidetone.ToString()}");
                        _deviceManager.SetSidetoneAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.Sidetone).Wait();
                        break;
                    case "TransparencylevelSlider":
                        _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetAncGainAsync ... Transparencylevel ... {DeviceInfoDTP.AncGain.ToString()}");
                        _deviceManager.SetAncGainAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.AncGain).Wait();
                        break;
                    //------------------------------------------------------------------------------------------
                    // Button類
                    case "DefaultCheck":
                    case "BassBoostCheck":
                    case "SpeechBoostCheck":
                    case "TrebleBoostCheck":
                    case "CustomCheck":
                        _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetSelectedPresetAsync ... {mode} ... {DeviceInfoDTP.SelectedPreset.ToString()}");
                        _deviceManager.SetSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.SelectedPreset).Wait();
                        break;
                    //------------------------------------------------------------------------------------------
                    case "WearDetectionCheck":
                        _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetWearDetectionAsync ... WearDetectionCheck ... {DeviceInfoDTP.WearDetectionFromDTP.ToString()}");
                        _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.WearDetectionFromDTP).Wait();
                        break;
                    case "PauseMusicCheck":
                    //_log.Info($"[HeadsetViewModel] SetIsWearDetectionPauseMusicEnabledAsync ... PauseMusicCheck ... {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()} ...");
                    //_deviceManager.SetIsWearDetectionPauseMusicEnabledAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP).Wait();
                    //break;
                    case "MuteMicrophoneCheck":
                    //_log.Info($"[HeadsetViewModel] SetIsWearDetectionMuteMicEnabledAsync ... MuteMicrophoneCheck ... {DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP.ToString()}");
                    //_deviceManager.SetIsWearDetectionMuteMicEnabledAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP).Wait();
                    //break;
                    // Button類
                    case "QuickPauseCheck":
                    case "NormalCheck":
                    case "SensitiveCheck":
                        _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetWearDetectionQuickPauseAsync ... {mode} ... {DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP.ToString()}");
                        _deviceManager.SetWearDetectionQuickPauseAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP).Wait();
                        break;
                    //------------------------------------------------------------------------------------------
                    // Button類
                    case "Normal2Check":
                    case "LowCheck":
                        _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetWearDetectionSensitivityAsync ... {mode} ... {DeviceInfoDTP.WearDetectionSensitivityFromDTP.ToString()}");
                        _deviceManager.SetWearDetectionSensitivityAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.WearDetectionSensitivityFromDTP).Wait();
                        break;
                    //------------------------------------------------------------------------------------------
                    case "AnswerCallsCheck":
                    //_log.Info($"[HeadsetViewModel] SetBoomMicAsync ... AnswerCallsCheck .... {DeviceInfoDTP.AnswerCall.ToString()}");
                    //_deviceManager.SetBoomMicAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.AnswerCall).Wait();
                    //break;
                    //------------------------------------------------------------------------------------------
                    case "BusyLightCheck":
                        //_log.Info($"[HeadsetViewModel] SetBusyLightAsync ....... {DeviceInfoDTP.BusyLight.ToString()}");
                        //_deviceManager.SetBusyLightAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.BusyLight).Wait();
                        break;
                    //------------------------------------------------------------------------------------------
                    // Button類
                    case "EssentialCheck":
                    case "AllCheck":
                        _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetVoiceGuidanceAsync ... {mode} .... {DeviceInfoDTP.BusyLight.ToString()}");
                        _deviceManager.SetVoiceGuidanceAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.VoiceGuidance).Wait();
                        break;
                    //------------------------------------------------------------------------------------------
                    case "OutgoingAudioCheck":
                    //_log.Info($"[HeadsetViewModel] SetMicNoiseCancellationAsync ... OutgoingAudioCheck .... {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
                    //_deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.MicNoiseCancellation).Wait();
                    //break;
                    case "IncomingAudioCheck":
                    //_log.Info($"[HeadsetViewModel] SetMicNCIncomingAsync ... IncomingAudioCheck .... {DeviceInfoDTP.MicNCIncoming.ToString()}");
                    //_deviceManager.SetMicNCIncomingAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.MicNCIncoming).Wait();
                    //break;
                    case "MicNoiseCancellationCheck":
                    //_log.Info($"[HeadsetViewModel] SetMicNoiseCancellationAsync ... MicNoiseCancellationCheck .... {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
                    //_deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.MicNoiseCancellation).Wait();
                    //break;
                    case "SidetoneCheck":
                        //_log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetSidetoneAsync ... SidetoneCheck .... {DeviceInfoDTP.Sidetone.ToString()}");
                        //_deviceManager.SetSidetoneAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.Sidetone).Wait();
                        break;
                    case "SidetoneSlider":
                        _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SidetoneLevel ... SidetoneLevel .... {DeviceInfoDTP.SidetoneLevel.ToString()}");
                        _deviceManager.SetSidetoneLevelAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.SidetoneLevel).Wait();
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
            _log.Info($"[HeadsetViewModel] ExecuteDebounced SetIsWearDetectionPauseMusicEnabledAsync ... PauseMusicCheck ... {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()} ...");
            _deviceManager.SetIsWearDetectionPauseMusicEnabledAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP).Wait();
        }

        private void ExecuteDebouncedActionForMuteMicrophone(object param)
        {
            _isRestoreEnable = false;
            _log.Info($"[HeadsetViewModel] ExecuteDebounced SetIsWearDetectionMuteMicEnabledAsync ... MuteMicrophoneCheck ... {DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP.ToString()}");
            _deviceManager.SetIsWearDetectionMuteMicEnabledAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP).Wait();
        }
        private void ExecuteDebouncedActionForQuickPause(object param)
        {
            _isRestoreEnable = false;
            _log.Info($"[HeadsetViewModel] ExecuteDebounced SetWearDetectionQuickPauseAsync ... QuickPauseCheck ... {DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP.ToString()}");
            _deviceManager.SetWearDetectionQuickPauseAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP).Wait();
        }
        private void ExecuteDebouncedActionForSidetoneCheck(object param)
        {
            _isRestoreEnable = false;
            _log.Info($"[HeadsetViewModel] ExecuteDebouncedAction SetSidetoneAsync ... SidetoneCheck .... {DeviceInfoDTP.Sidetone.ToString()}");
            _deviceManager.SetSidetoneAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.Sidetone).Wait();
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
            _log!.Info($"[HeadsetViewModel] DetectPageShow ... {model}");
            //modelTest = model;
            ReadQRCodeReg();
            AllResetHeadsetPage();
            string fwv = string.Empty;
            bool answerCall = false;
            if (!IsDTPReady)
            {
                fwv = _deviceManager.GetHeadsetFirmwareVersionAsync(CurrentDeviceInfo!.ID.ToString()).Result;
                answerCall = _deviceManager.GetIsBoomMicSupportedAsync(CurrentDeviceInfo.ID.ToString()).Result; //DTP
            }
            else
            {
                fwv = FirmwareVersion;
                answerCall = SupportedAnswerCalls;
            }
            _log!.Info($"[HeadsetViewModel] DetectPageShow ... BoomMicSupported = {answerCall.ToString()}");
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
                    //_deviceSettingsDownloadDellAudioPageShow = false;
                    break;

                case "WL5024"://Pegasus
                    //Page 1
                    _controlTheNoiseIHearPageShow = true;
                    _configureMyAudioModesPageShow = true;
                    //Page 2
                    _wearDetectionPageShow = true;
                    _automatedActionsSensitivityUpPageShow = true;
                    _automatedActionsWhenHeadsetIsRemovedPageShow = true;
                    _automatedActionsAnswerCallPageShow = answerCall; //WL5024 page2, not only AnswerCall 
                    //Page 3
                    _voiceGuidancePageShow = true;
                    //_deviceSettingsDownloadDellAudioPageShow = false;
                    break;

                case "WH5024"://Winflo
                    //Page 1
                    _controlTheNoiseIHearPageShow = true;
                    _configureMyAudioModesPageShow = true;
                    //Page 2                  
                    //if (ConvertVersionToInt(fwv) >= 252)
                    //    _automatedActionsAnswerCallPageShow = true;
                    //else
                    _automatedActionsAnswerCallPageShow = answerCall;//DELL 說拿掉;// only AnswerCall 
                    //Page 3
                    _voiceGuidancePageShow = true;
                    _deviceSettingsDownloadDellAudioPageShow = false;
                    break;

                case "WL3024"://Vaporify
                    //Page 1
                    _configureMyAudioModesPageShow = true;
                    //Page 2
                    _automatedActionsAnswerCallPageShow = answerCall;// only AnswerCall 
                    //Page 3
                    _voiceGuidancePageShow = true;
                    //_deviceSettingsDownloadDellAudioPageShow = false;
                    break;

                case "WH3024"://Airmax
                    //Page 1
                    _controlTheNoiseIHearPageShow = false;//Fix PIMS-PIMS-294568
                    _configureMyAudioModesPageShow = true;
                    //Page 2
                    //if (ConvertVersionToInt(fwv) >= 275)
                    //    _automatedActionsAnswerCallPageShow = true;//DELL 說拿掉;
                    //else
                    _automatedActionsAnswerCallPageShow = answerCall;//DELL 說拿掉;// only AnswerCall 
                    //Page 3
                    _deviceSettingsDownloadDellAudioPageShow = false;
                    //defult page
                    break;

                default:
                    break;
            }
            CheckHeadsetFunc();
        }
        public void ReadQRCodeReg()
        {
            object regValue = null;
            try
            {
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    regValue = DdpmCommonHelper.DeviceManagerSA!.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, RegPath, RegKeyForQRCode).Result;

                    if (regValue != null)
                    {

                        if (Convert.ToBoolean(regValue))
                        {
                            _deviceSettingsDownloadDellAudioPageShow = false;
                            _log.Info($"[HeadsetViewModel] ReadQRCodeReg ....... success true");
                        }
                        else
                        {
                            _deviceSettingsDownloadDellAudioPageShow = true;
                            _log.Info($"[HeadsetViewModel] ReadQRCodeReg ....... success false");
                        }

                    }
                    else
                    {
                        _deviceSettingsDownloadDellAudioPageShow = true;
                        _log.Info($"[HeadsetViewModel] ReadQRCodeReg ReadRegistryData ....... fail");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Info($"[HeadsetViewModel] ReadQRCodeReg ....... {ex.ToString()}");
            }
        }


        public void AllResetHeadsetPage()
        {
            _log.Info($"[HeadsetViewModel] AllResetHeadsetPage ...");
            _controlTheNoiseIHearPageShow = false;
            _configureMyAudioModesPageShow = false;
            _wearDetectionPageShow = false;
            _automatedActionsWhenHeadsetIsRemovedPageShow = false;
            _automatedActionsQuickPausePageShow = false;
            _automatedActionsSensitivityPageShow = false;
            _voiceGuidancePageShow = false;
            //_deviceSettingsDownloadDellAudioPageShow = false;
            _automatedActionsSensitivityUpPageShow = false;
            _automatedActionsAnswerCallPageShow = false;
        }

        public void CheckHeadsetFunc()
        {
            _log.Info($"[HeadsetViewModel] CheckHeadsetFunc ...");
            CheckSidetoneUI(false);
            CheckBusyLightUI(false);
            CheckOutgoingAudioUI(false);
            CheckMicNCIncomingUI(false);
            CheckMicNoiseCancellationUI(false);
            CheckWearDetectionUI(false);
            CheckPresetsUI(false);
            CheckVoiceGuidanceUI(false);
            CheckANCUI(false);
            CheckAnswerCallUI(false);
            HeadsetSettingChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CheckSidetoneUI(bool PropertyChange)
        {
            if (DeviceInfoDTP!.IsSidetoneSupported)
            {
                _isSidetoneStatus = DeviceInfoDTP.Sidetone;//_deviceManager.GetSidetoneAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo.Sidetone;

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
            if (DeviceInfoDTP!.IsSidetoneSupported)
            {
                int sidevalue = DeviceInfoDTP.SidetoneLevel;//_deviceManager.GetSidetoneLevelAsync(CurrentDeviceInfo!.ID.ToString()).Result;
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
            if (DeviceInfoDTP!.IsAnswerCallSupported)
                _isAnswerCallsStatus = DeviceInfoDTP.AnswerCall;
            if (PropertyChange)
            {
                OnPropertyChanged(nameof(AnswerCallsStatus));
                OnPropertyChanged(nameof(AnswerCalls_String));
            }
        }

        private void CheckBusyLightUI(bool PropertyChange)
        {
            if (DeviceInfoDTP!.IsBusyLightSupported)
                _isBusyLightStatus = DeviceInfoDTP.BusyLight;//_deviceManager.GetBusyLightAsync(CurrentDeviceInfo!.ID.ToString()).Result; //CurrentDeviceInfo.BusyLight;
            if (PropertyChange)
            {
                OnPropertyChanged(nameof(BusyLightStatus));
                OnPropertyChanged(nameof(BusyLight_String));
            }
        }
        private void CheckOutgoingAudioUI(bool PropertyChange)
        {
            if (DeviceInfoDTP!.IsMicNoiseCancellationSupported)
            {
                if (_isOutgoingAudioStatus != DeviceInfoDTP.MicNoiseCancellation)
                {
                    _isOutgoingAudioStatus = DeviceInfoDTP.MicNoiseCancellation;

                    if (PropertyChange)
                    {
                        OnPropertyChanged(nameof(OutgoingAudioStatus));
                        OnPropertyChanged(nameof(OutgoingAudio_String));
                        UpdateCollaborationAndultimediaUI(true, false);
                    }
                }
            }
        }

        private void CheckMicNCIncomingUI(bool PropertyChange)
        {
            if (DeviceInfoDTP!.IsMicNCIncomingSupported)
            {
                if (_isIncomingAudioStatus != DeviceInfoDTP.MicNCIncoming)
                {
                    _isIncomingAudioStatus = DeviceInfoDTP.MicNCIncoming;

                    if (PropertyChange)
                    {
                        OnPropertyChanged(nameof(IncomingAudioStatus));
                        OnPropertyChanged(nameof(IncomingAudio_String));
                        UpdateCollaborationAndultimediaUI(true, false);
                    }
                }
            }
        }

        private void CheckMicNoiseCancellationUI(bool PropertyChange)
        {
            if (DeviceInfoDTP!.IsMicNoiseCancellationSupported)
                _isMicNoiseCancellationStatus = DeviceInfoDTP.MicNoiseCancellation;

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
            if (DeviceInfoDTP!.IsWearDetectionSupported)
            {
                _isWearDetectionStatus = DeviceInfoDTP.WearDetectionFromDTP;
                _isPauseMusicStatus = DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP;
                _isMuteMicrophoneStatus = DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP;
                _isQuickPauseStatus = IntToBool(DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP);

                if (Model == "WL7024")
                {
                    if (DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP == 0)
                    {
                        _isQuickPauseStatus = false;
                        _isNormalChecked = true;
                        _isSensitiveChecked = false;
                    }
                    if (DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP == 2)
                    {
                        _isQuickPauseStatus = true;
                        _isNormalChecked = false;
                        _isSensitiveChecked = true;
                    }
                    if (DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP == 1)
                    {
                        if (!_isQuickPauseStatus)
                        {
                            _isQuickPauseStatus = !_isQuickPauseStatus;
                        }
                        _isNormalChecked = true;
                        _isSensitiveChecked = false;
                    }
                }

                if (IntToBool(DeviceInfoDTP.WearDetectionSensitivityFromDTP))
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
            if (DeviceInfoDTP!.IsPresetsSupported)
            {
                switch (DeviceInfoDTP.SelectedPreset)
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
            if (DeviceInfoDTP!.VoiceGuidance)
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
            if (DeviceInfoDTP!.IsANCSupported)
            {
                switch (DeviceInfoDTP.AncMode)
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
                        _isTransparencylevelSliderValue = DeviceInfoDTP.AncGain;
                        break;
                }

                if (PropertyChange)
                {
                    OnPropertyChanged(nameof(IsNoiseOffChecked));
                    OnPropertyChanged(nameof(IsActiveNoiseCancellingChecked));
                    OnPropertyChanged(nameof(IsTransparencyChecked));
                    OnPropertyChanged(nameof(TransparencylevelSliderValue));
                }
            }
        }

        private void UpdateCollaborationAndultimediaUI(bool Collaboration, bool Multimedia)
        {
            _isCollaborationChecked = Collaboration;
            _isMultimediaChecked = Multimedia;
            OnPropertyChanged(nameof(IsCollaborationChecked));
            OnPropertyChanged(nameof(IsMultimediaChecked));
        }

        private void UpdateResetToDefault()
        {
            if (CheckIfCurrentSettingsMatchDefault(DeviceInfoDTP, Model))
                _isRestoreEnable = true;
            else
                _isRestoreEnable = false;
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
            _log.Info($"[HeadsetViewModel] SetCurrentDevice ...");
            if (!base.SetCurrentDevice(deviceID))
                return false;
            _log.Info($"[HeadsetViewModel] SetCurrentDevice GUID ... {CurrentDeviceID.ToString()}");

            _current_headset = CurrentDeviceID!.ToString();
            var fv = _deviceManager.GetHeadsetFirmwareVersionAsync(CurrentDeviceID.ToString()).Result;
            if (fv == null || fv == string.Empty)
            {
                _log.Info($"[HeadsetViewModel] SetCurrentDevice ... GetHeadsetFirmwareVersionAsync ... Null or Empty ... DTP fail ...");
                IsDTPReady = false;
                fv = CurrentDeviceInfo.FirmwareVersion.PadLeft(4, '0');
                FirmwareVersion2 = $"Firmware Version {fv}";
            }
            else
            {
                IsDTPReady = true;
                FirmwareVersion2 = Strings.FirmwareVersion + $" {fv}";
            }
            return true;
        }

        public override void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            _log.Info($"[HeadsetViewModel] HandleNotification ... Receive {property.ToString()}");
            base.HandleNotification(changeType, di, property);
            //CheckHeadsetFunc();
            switch (property)
            {
                //case "IsReadyChanged":
                //    break;
                //case "IsDirtyChanged":
                //    break;
                case "MicNoiseCancellationChanged":
                    DeviceInfoDTP.MicNoiseCancellation = di.MicNoiseCancellation;//_deviceManager.GetMicNoiseCancellationAsync(CurrentDeviceID.ToString()).Result;
                    CheckMicNoiseCancellationUI(true);
                    CheckOutgoingAudioUI(true);
                    break;

                case "MicNCIncomingChanged":
                    DeviceInfoDTP.MicNCIncoming = di.MicNCIncoming;//_deviceManager.GetMicNCIncomingAsync(CurrentDeviceID.ToString()).Result;
                    CheckMicNCIncomingUI(true);
                    break;

                case "SidetoneChanged":
                    DeviceInfoDTP.Sidetone = di.Sidetone;//_deviceManager.GetSidetoneAsync(CurrentDeviceID.ToString()).Result;
                    CheckSidetoneUI(true);
                    break;

                case "BusyLightChanged":
                    DeviceInfoDTP.BusyLight = di.BusyLight;//_deviceManager.GetBusyLightAsync(CurrentDeviceID.ToString()).Result;
                    CheckBusyLightUI(true);
                    break;

                case "VoiceGuidanceChanged":
                    DeviceInfoDTP.VoiceGuidance = di.VoiceGuidance;//_deviceManager.GetVoiceGuidanceAsync(CurrentDeviceID.ToString()).Result;
                    CheckVoiceGuidanceUI(true);
                    break;

                case "SelectedPresetChanged":
                    DeviceInfoDTP.SelectedPreset = di.SelectedPreset;//_deviceManager.GetSelectedPresetAsync(CurrentDeviceID.ToString()).Result;
                    CheckPresetsUI(true);
                    break;

                case "SidetoneLevelChanged":
                    DeviceInfoDTP.SidetoneLevel = di.SidetoneLevel;//_deviceManager.GetSidetoneLevelAsync(CurrentDeviceID.ToString()).Result;
                    CheckSidetoneLevelUI(true);
                    break;
                //case "MuteStatusChanged":
                //    break;
                case "BandsGainChanged":
                    DeviceInfoDTP.Band1Gain = di.Band1Gain;
                    DeviceInfoDTP.Band2Gain = di.Band2Gain;
                    DeviceInfoDTP.Band3Gain = di.Band3Gain;
                    DeviceInfoDTP.Band4Gain = di.Band4Gain;
                    DeviceInfoDTP.Band5Gain = di.Band5Gain;
                    HeadsetSettingChanged?.Invoke(this, EventArgs.Empty);
                    break;

                case "AncModeChanged":
                    DeviceInfoDTP.AncMode = di.AncMode;//_deviceManager.GetAncModeAsync(CurrentDeviceID.ToString()).Result;
                    CheckANCUI(true);
                    break;

                case "AncGainChanged":
                    DeviceInfoDTP.AncGain = di.AncGain;
                    _isTransparencylevelSliderValue = di.AncGain;
                    OnPropertyChanged(nameof(TransparencylevelSliderValue));
                    break;

                case "WearDetectionChanged":
                    DeviceInfoDTP.WearDetection = di.WearDetection;//_deviceManager.GetWearDetectionAsync(CurrentDeviceID.ToString()).Result;
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
                if (IsDTPReady)
                {
                    _log.Info($"[HeadsetViewModel] Print before property ...RestoreToDefault ... in");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band1Gain ...........= {DeviceInfoDTP.Band1Gain.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band2Gain ...........= {DeviceInfoDTP.Band2Gain.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band3Gain ...........= {DeviceInfoDTP.Band3Gain.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band4Gain ...........= {DeviceInfoDTP.Band4Gain.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band5Gain ...........= {DeviceInfoDTP.Band5Gain.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.AncGain .............= {DeviceInfoDTP.AncGain.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.AncMode .............= {DeviceInfoDTP.AncMode.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.BatteryLevel ........= {DeviceInfoDTP.BatteryLevel.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.BusyLight ...........= {DeviceInfoDTP.BusyLight.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.MicNoiseCancellation = {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.MicNCIncoming .......= {DeviceInfoDTP.MicNCIncoming.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Sidetone ............= {DeviceInfoDTP.Sidetone.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.SidetoneLevel .......= {DeviceInfoDTP.SidetoneLevel.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.SelectedPreset ......= {DeviceInfoDTP.SelectedPreset.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.VoiceGuidance .......= {DeviceInfoDTP.VoiceGuidance.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.WearDetection .......= {DeviceInfoDTP.WearDetection.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.AnswerCall ..........= {DeviceInfoDTP.AnswerCall.ToString()}");
                    _deviceManager.SetFactoryResetAsyncValueForHeadset(CurrentDeviceInfo!.ID.ToString(), true).Wait();
                    UpdateDTPValue();
                    //_showPluginManager = HeadsetPlugin.PluginIoc.GetService<IShowPluginManager>();
                    //_showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.HeadsetPluginId, CurrentDeviceInfo!.ID.ToString());
                }
                else
                {
                    _log.Info($"[HeadsetViewModel] DTH Print before property ...RestoreToDefault ... in");
                    SetFactoryResetForDTH();
                    UpdateDTHValue();
                }
                CheckHeadsetFunc();
                UpdateResetToDefault();
                HeadsetSettingChanged?.Invoke(this, EventArgs.Empty);
                //_showPluginManager?.ShowHomePage();
            }
            catch (Exception ex)
            {
                _log!.Error($"[HeadsetViewModel] RestoreToDefault ...... {ex.ToString()}");
            }
        }

        private void SetFactoryResetForDTH()
        {
            try
            {
                _log.Info($"[HeadsetViewModel] DTH SetFactoryResetForDTH ......");
                _log.Info($"[HeadsetViewModel] DTH ***********************************************************************");

                //object varr = await _deviceManager.GetHeadsetDeviceItemsExAsync();

                //Read this Headset Support function
                if (CurrentDeviceInfo!.IsANCSupported)
                {
                    DeviceInfoDTP.IsANCSupported = true;
                    DeviceInfoDTP.AncMode = 1;
                    DeviceInfoDTP.AncGain = 1;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.AncMode .....................= {DeviceInfoDTP.AncMode.ToString()}");
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.AncGain .....................= {DeviceInfoDTP.AncGain.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsANCSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsANCSupportedAsync .............................. NO");
                }
                //------------------------------------------------------------------------------------
                if (CurrentDeviceInfo!.IsBusyLightSupported)
                {
                    DeviceInfoDTP.IsBusyLightSupported = true;
                    DeviceInfoDTP.BusyLight = true;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.BusyLight ...................= {DeviceInfoDTP.BusyLight.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsBusyLightSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsBusyLightSupportedAsync ........................ NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsMicNoiseCancellationSupported)
                {
                    DeviceInfoDTP.IsMicNoiseCancellationSupported = true;
                    DeviceInfoDTP.MicNoiseCancellation = true;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.MicNoiseCancellation .............= {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsMicNoiseCancellationSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsMicNoiseCancellationSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsSidetoneSupported)
                {
                    DeviceInfoDTP.IsSidetoneSupported = true;
                    DeviceInfoDTP.Sidetone = true;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Sidetone ...................= {DeviceInfoDTP.Sidetone.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsSidetoneSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsSidetoneSupportedAsync ......................... NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsVoiceGuidanceSupported)
                {
                    DeviceInfoDTP.IsVoiceGuidanceSupported = true;
                    DeviceInfoDTP.VoiceGuidance = true;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.VoiceGuidance ..............= {DeviceInfoDTP.VoiceGuidance.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsVoiceGuidanceSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsVoiceGuidanceSupportedAsync .................... NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsPresetsSupported)
                {
                    DeviceInfoDTP.IsPresetsSupported = true;
                    DeviceInfoDTP.SelectedPreset = CurrentDeviceInfo!.SelectedPreset;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.SelectedPreset .............= {DeviceInfoDTP.SelectedPreset.ToString()}");
                    if (CurrentDeviceInfo!.IsEqualizerSupported)
                    {
                        DeviceInfoDTP.IsEqualizerSupported = true;
                        DeviceInfoDTP.Band1Gain = 0;
                        DeviceInfoDTP.Band2Gain = 0;
                        DeviceInfoDTP.Band3Gain = 0;
                        DeviceInfoDTP.Band4Gain = 0;
                        DeviceInfoDTP.Band5Gain = 0;
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band1Gain ..................= {DeviceInfoDTP.Band1Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band2Gain ..................= {DeviceInfoDTP.Band2Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band3Gain ..................= {DeviceInfoDTP.Band3Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band4Gain ..................= {DeviceInfoDTP.Band4Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band5Gain ..................= {DeviceInfoDTP.Band5Gain.ToString()}");
                    }
                    else
                    {
                        DeviceInfoDTP.IsEqualizerSupported = false;
                        _log.Info($"[HeadsetViewModel] DTH GetIsEqualizerSupportedAsync .................... NO");
                    }
                }
                else
                {
                    DeviceInfoDTP.IsPresetsSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsPresetsSupportedAsync ................... NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsMicNCIncomingSupported)
                {
                    DeviceInfoDTP.IsMicNCIncomingSupported = true;
                    DeviceInfoDTP.MicNCIncoming = true;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.MicNCIncoming .............= {DeviceInfoDTP.MicNCIncoming.ToString()}");
                }
                else
                {
                    _log.Info($"[HeadsetViewModel] DTH GetIsMicNCIncomingSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------
                DeviceInfoDTP.SidetoneLevel = CurrentDeviceInfo!.SidetoneLevel;
            }
            catch (Exception ex)
            {
                _log!.Error($"[HeadsetViewModel] DTH SetFactoryResetForDTH ...... {ex.ToString()}");
            }
        }

        /// <summary>
        /// Update DTP Headset property Value
        /// </summary>
        /// <returns></returns>
        private void UpdateDTPValue()
        {
            try
            {
                _log.Info($"[HeadsetViewModel] Print before property ...UpdateDTPValue ... in");

                if (DeviceInfoDTP == null)
                {
                    DeviceInfoDTP = new DeviceInfoDTP();
                    _log.Info($"[HeadsetViewModel] Print before property ...UpdateDTPValue new DeviceInfo...");
                }

                _log.Info($"[HeadsetViewModel] Print after property ......");
                _log.Info($"[HeadsetViewModel] ***********************************************************************");

                //object varr = await _deviceManager.GetHeadsetDeviceItemsExAsync();

                //Read this Headset Support function
                if (_deviceManager.GetIsANCSupportedAsync(CurrentDeviceID.ToString()).Result)
                {
                    DeviceInfoDTP.IsANCSupported = true;
                    DeviceInfoDTP.AncMode = _deviceManager.GetAncModeAsync(CurrentDeviceID.ToString()).Result;
                    DeviceInfoDTP.AncGain = _deviceManager.GetAncGainAsync(CurrentDeviceID.ToString()).Result;
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.AncMode .....................= {DeviceInfoDTP.AncMode.ToString()}");
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.AncGain .....................= {DeviceInfoDTP.AncGain.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsANCSupported = false;
                    _log.Info($"[HeadsetViewModel] GetIsANCSupportedAsync .............................. NO");
                }
                //------------------------------------------------------------------------------------
                if (_deviceManager.GetIsBusyLightSupportedAsync(CurrentDeviceID.ToString()).Result)
                {
                    DeviceInfoDTP.IsBusyLightSupported = true;
                    DeviceInfoDTP.BusyLight = _deviceManager.GetBusyLightAsync(CurrentDeviceID.ToString()).Result;
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.BusyLight ...................= {DeviceInfoDTP.BusyLight.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsBusyLightSupported = false;
                    _log.Info($"[HeadsetViewModel] GetIsBusyLightSupportedAsync ........................ NO");
                }
                //------------------------------------------------------------------------------------

                if (_deviceManager.GetIsMicNoiseCancellationSupportedAsync(CurrentDeviceID.ToString()).Result)
                {
                    DeviceInfoDTP.IsMicNoiseCancellationSupported = true;
                    DeviceInfoDTP.MicNoiseCancellation = _deviceManager.GetMicNoiseCancellationAsync(CurrentDeviceID.ToString()).Result;
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.MicNoiseCancellation .............= {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsMicNoiseCancellationSupported = false;
                    _log.Info($"[HeadsetViewModel] GetIsMicNoiseCancellationSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------

                if (_deviceManager.GetIsSidetoneSupportedAsync(CurrentDeviceID.ToString()).Result)
                {
                    DeviceInfoDTP.IsSidetoneSupported = true;
                    DeviceInfoDTP.Sidetone = _deviceManager.GetSidetoneAsync(CurrentDeviceID.ToString()).Result;
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Sidetone ...................= {DeviceInfoDTP.Sidetone.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsSidetoneSupported = false;
                    _log.Info($"[HeadsetViewModel] GetIsSidetoneSupportedAsync ......................... NO");
                }
                //------------------------------------------------------------------------------------

                if (_deviceManager.GetIsVoiceGuidanceSupportedAsync(CurrentDeviceID.ToString()).Result)
                {
                    DeviceInfoDTP.IsVoiceGuidanceSupported = true;
                    DeviceInfoDTP.VoiceGuidance = _deviceManager.GetVoiceGuidanceAsync(CurrentDeviceID.ToString()).Result;
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.VoiceGuidance ..............= {DeviceInfoDTP.VoiceGuidance.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsVoiceGuidanceSupported = false;
                    _log.Info($"[HeadsetViewModel] GetIsVoiceGuidanceSupportedAsync .................... NO");
                }
                //------------------------------------------------------------------------------------

                if (_deviceManager.GetIsPresetsSupportedAsync(CurrentDeviceID.ToString()).Result)
                {
                    DeviceInfoDTP.IsPresetsSupported = true;
                    DeviceInfoDTP.SelectedPreset = _deviceManager.GetSelectedPresetAsync(CurrentDeviceID.ToString()).Result;
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.SelectedPreset .............= {DeviceInfoDTP.SelectedPreset.ToString()}");
                    if (_deviceManager.GetIsEqualizerSupportedAsync(CurrentDeviceID.ToString()).Result)
                    {
                        DeviceInfoDTP.IsEqualizerSupported = true;
                        DeviceInfoDTP.Band1Gain = _deviceManager.GetBand1GainAsync(CurrentDeviceID.ToString()).Result;
                        DeviceInfoDTP.Band2Gain = _deviceManager.GetBand2GainAsync(CurrentDeviceID.ToString()).Result;
                        DeviceInfoDTP.Band3Gain = _deviceManager.GetBand3GainAsync(CurrentDeviceID.ToString()).Result;
                        DeviceInfoDTP.Band4Gain = _deviceManager.GetBand4GainAsync(CurrentDeviceID.ToString()).Result;
                        DeviceInfoDTP.Band5Gain = _deviceManager.GetBand5GainAsync(CurrentDeviceID.ToString()).Result;
                        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band1Gain ..................= {DeviceInfoDTP.Band1Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band2Gain ..................= {DeviceInfoDTP.Band2Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band3Gain ..................= {DeviceInfoDTP.Band3Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band4Gain ..................= {DeviceInfoDTP.Band4Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.Band5Gain ..................= {DeviceInfoDTP.Band5Gain.ToString()}");
                    }
                    else
                    {
                        DeviceInfoDTP.IsEqualizerSupported = false;
                        _log.Info($"[HeadsetViewModel] GetIsEqualizerSupportedAsync .................... NO");
                    }
                }
                else
                {
                    DeviceInfoDTP.IsPresetsSupported = false;
                    _log.Info($"[HeadsetViewModel] GetIsPresetsSupportedAsync ................... NO");
                }
                //------------------------------------------------------------------------------------

                if (_deviceManager.GetIsMicNCIncomingSupportedAsync(CurrentDeviceID.ToString()).Result)
                {
                    DeviceInfoDTP.IsMicNCIncomingSupported = true;
                    DeviceInfoDTP.MicNCIncoming = _deviceManager.GetMicNCIncomingAsync(CurrentDeviceID.ToString()).Result;
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.MicNCIncoming .............= {DeviceInfoDTP.MicNCIncoming.ToString()}");
                }
                else
                {
                    _log.Info($"[HeadsetViewModel] GetIsMicNCIncomingSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------

                if (_deviceManager.GetIsWearDetectionSupportedAsync(CurrentDeviceID.ToString()).Result)
                {
                    DeviceInfoDTP.IsWearDetectionSupported = true;
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.GetIsWearDetectionSupportedAsync .............= {DeviceInfoDTP.IsWearDetectionSupported.ToString()}");
                    if (DeviceInfoDTP.IsWearDetectionSupported)
                    {
                        DeviceInfoDTP.WearDetectionFromDTP = _deviceManager.GetWearDetectionAsync(CurrentDeviceID.ToString()).Result;
                        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.WearDetectionFromDTP .............= {DeviceInfoDTP.WearDetectionFromDTP.ToString()}");

                        DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = _deviceManager.GetIsWearDetectionPauseMusicEnabledAsync(CurrentDeviceID.ToString()).Result;
                        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");

                        DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = _deviceManager.GetIsWearDetectionMuteMicEnabledAsync(CurrentDeviceID.ToString()).Result;
                        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");

                        DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = _deviceManager.GetWearDetectionQuickPauseAsync(CurrentDeviceID.ToString()).Result;
                        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");

                        DeviceInfoDTP.WearDetectionSensitivityFromDTP = _deviceManager.GetWearDetectionSensitivityAsync(CurrentDeviceID.ToString()).Result;
                        _log.Info($"[HeadsetViewModel] DeviceInfoDTP.WearDetectionSensitivityFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
                    }
                }
                else
                {
                    DeviceInfoDTP.IsWearDetectionSupported = false;
                    _log.Info($"[HeadsetViewModel] GetIsWearDetectionSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------

                if (_deviceManager.GetIsBoomMicSupportedAsync(CurrentDeviceID.ToString()).Result)
                {
                    DeviceInfoDTP.IsAnswerCallSupported = true;
                    DeviceInfoDTP.AnswerCall = _deviceManager.GetBoomMicAsync(CurrentDeviceID.ToString()).Result;
                    _log.Info($"[HeadsetViewModel] DeviceInfoDTP.AnswerCall .............= {DeviceInfoDTP.AnswerCall.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsAnswerCallSupported = false;
                    _log.Info($"[HeadsetViewModel] GetIsBoomMicSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------

                //DeviceInfoDTP.BatteryLevel = await _deviceManager.GetHeadsetBatteryLevelAsync(CurrentDeviceID.ToString());
                DeviceInfoDTP.SidetoneLevel = _deviceManager.GetSidetoneLevelAsync(CurrentDeviceID.ToString()).Result;
                PairedHostName1 = _deviceManager.GetHeadsetPairedHostName2Async(CurrentDeviceInfo.ID.ToString()).Result;
                PairedHostName2 = _deviceManager.GetHeadsetPairedHostName3Async(CurrentDeviceInfo.ID.ToString()).Result;

                UpdateResetToDefault();
                //OnPropertyChanged(nameof(IsRestoreEnable));
            }
            catch (Exception ex)
            {
                _log!.Error($"[HeadsetViewModel] UpdateDTPValue ...... {ex.ToString()}");
            }
        }

        /// <summary>
        /// Update DTH Headset property Value
        /// </summary>
        /// <returns></returns>
        private void UpdateDTHValue()
        {
            try
            {
                _log.Info($"[HeadsetViewModel] DTH Print before property ...UpdateDTHValue ... in");

                if (DeviceInfoDTP == null)
                {
                    DeviceInfoDTP = new DeviceInfoDTP();
                    _log.Info($"[HeadsetViewModel] DTH Print before property ...UpdateDTPValue new DeviceInfo...");
                }

                _log.Info($"[HeadsetViewModel] DTH Print after property ......");
                _log.Info($"[HeadsetViewModel] DTH ***********************************************************************");

                //object varr = await _deviceManager.GetHeadsetDeviceItemsExAsync();

                //Read this Headset Support function
                if (CurrentDeviceInfo!.IsANCSupported)
                {
                    DeviceInfoDTP.IsANCSupported = true;
                    DeviceInfoDTP.AncMode = CurrentDeviceInfo!.AncMode;
                    DeviceInfoDTP.AncGain = CurrentDeviceInfo!.AncGain;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.AncMode .....................= {DeviceInfoDTP.AncMode.ToString()}");
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.AncGain .....................= {DeviceInfoDTP.AncGain.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsANCSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsANCSupportedAsync .............................. NO");
                }
                //------------------------------------------------------------------------------------
                if (CurrentDeviceInfo!.IsBusyLightSupported)
                {
                    DeviceInfoDTP.IsBusyLightSupported = true;
                    DeviceInfoDTP.BusyLight = CurrentDeviceInfo!.BusyLight;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.BusyLight ...................= {DeviceInfoDTP.BusyLight.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsBusyLightSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsBusyLightSupportedAsync ........................ NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsMicNoiseCancellationSupported)
                {
                    DeviceInfoDTP.IsMicNoiseCancellationSupported = true;
                    DeviceInfoDTP.MicNoiseCancellation = CurrentDeviceInfo!.MicNoiseCancellation;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.MicNoiseCancellation .............= {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsMicNoiseCancellationSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsMicNoiseCancellationSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsSidetoneSupported)
                {
                    DeviceInfoDTP.IsSidetoneSupported = true;
                    DeviceInfoDTP.Sidetone = CurrentDeviceInfo!.Sidetone;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Sidetone ...................= {DeviceInfoDTP.Sidetone.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsSidetoneSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsSidetoneSupportedAsync ......................... NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsVoiceGuidanceSupported)
                {
                    DeviceInfoDTP.IsVoiceGuidanceSupported = true;
                    DeviceInfoDTP.VoiceGuidance = CurrentDeviceInfo!.VoiceGuidance;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.VoiceGuidance ..............= {DeviceInfoDTP.VoiceGuidance.ToString()}");
                }
                else
                {
                    DeviceInfoDTP.IsVoiceGuidanceSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsVoiceGuidanceSupportedAsync .................... NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsPresetsSupported)
                {
                    DeviceInfoDTP.IsPresetsSupported = true;
                    DeviceInfoDTP.SelectedPreset = CurrentDeviceInfo!.SelectedPreset;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.SelectedPreset .............= {DeviceInfoDTP.SelectedPreset.ToString()}");
                    if (CurrentDeviceInfo!.IsEqualizerSupported)
                    {
                        DeviceInfoDTP.IsEqualizerSupported = true;
                        DeviceInfoDTP.Band1Gain = CurrentDeviceInfo!.Band1Gain;
                        DeviceInfoDTP.Band2Gain = CurrentDeviceInfo!.Band2Gain;
                        DeviceInfoDTP.Band3Gain = CurrentDeviceInfo!.Band3Gain;
                        DeviceInfoDTP.Band4Gain = CurrentDeviceInfo!.Band4Gain;
                        DeviceInfoDTP.Band5Gain = CurrentDeviceInfo!.Band5Gain;
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band1Gain ..................= {DeviceInfoDTP.Band1Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band2Gain ..................= {DeviceInfoDTP.Band2Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band3Gain ..................= {DeviceInfoDTP.Band3Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band4Gain ..................= {DeviceInfoDTP.Band4Gain.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.Band5Gain ..................= {DeviceInfoDTP.Band5Gain.ToString()}");
                    }
                    else
                    {
                        DeviceInfoDTP.IsEqualizerSupported = false;
                        _log.Info($"[HeadsetViewModel] DTH GetIsEqualizerSupportedAsync .................... NO");
                    }
                }
                else
                {
                    DeviceInfoDTP.IsPresetsSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsPresetsSupportedAsync ................... NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsMicNCIncomingSupported)
                {
                    DeviceInfoDTP.IsMicNCIncomingSupported = true;
                    DeviceInfoDTP.MicNCIncoming = CurrentDeviceInfo!.MicNCIncoming;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.MicNCIncoming .............= {DeviceInfoDTP.MicNCIncoming.ToString()}");
                }
                else
                {
                    _log.Info($"[HeadsetViewModel] DTH GetIsMicNCIncomingSupportedAsync ............. NO");
                }
                //------------------------------------------------------------------------------------

                if (CurrentDeviceInfo!.IsWearDetectionSupported)
                {
                    DeviceInfoDTP.IsWearDetectionSupported = true;
                    _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.GetIsWearDetectionSupportedAsync .............= {DeviceInfoDTP.IsWearDetectionSupported.ToString()}");
                    if (DeviceInfoDTP.IsWearDetectionSupported)
                    {
                        uint wearDetectionValue = (uint)CurrentDeviceInfo!.WearDetection;
                        if (GetBitValue(wearDetectionValue, 0) == 1)
                            DeviceInfoDTP.WearDetectionFromDTP = true;
                        else
                            DeviceInfoDTP.WearDetectionFromDTP = false;

                        if (GetBitValue(wearDetectionValue, 1) == 1)
                            DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = true;
                        else
                            DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = false;

                        if (GetBitValue(wearDetectionValue, 2) == 1)
                            DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = true;
                        else
                            DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = false;

                        if (GetBitValue(wearDetectionValue, 4) == 1)
                            DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 1;
                        else
                            DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 0;

                        //7024
                        if (CurrentDeviceInfo!.ModelNumber.Contains("7024"))
                        {
                            if (GetBitsValue(wearDetectionValue, 5) == 1)
                            {
                                DeviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
                                DeviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
                            }
                            else
                            {
                                DeviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
                                DeviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
                            }
                        }

                        //5024
                        if (CurrentDeviceInfo!.ModelNumber.Contains("5024"))
                        {
                            if (GetBitValue(wearDetectionValue, 3) == 1)
                            {
                                DeviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
                                DeviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
                            }
                            else
                            {
                                DeviceInfoDTP.WearDetectionSensitivityFromDTP = 0;
                                DeviceInfoDTP.WearDetectionSensitivityFromDTP = 1;
                            }
                        }
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.WearDetectionFromDTP .............= {DeviceInfoDTP.WearDetectionFromDTP.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
                        _log.Info($"[HeadsetViewModel] DTH DeviceInfoDTP.WearDetectionSensitivityFromDTP .............= {DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.ToString()}");
                    }
                }
                else
                {
                    DeviceInfoDTP.IsWearDetectionSupported = false;
                    _log.Info($"[HeadsetViewModel] DTH GetIsWearDetectionSupportedAsync ............. NO");
                }

                DeviceInfoDTP.SidetoneLevel = CurrentDeviceInfo!.SidetoneLevel;

                UpdateResetToDefault();
                //OnPropertyChanged(nameof(IsRestoreEnable));
            }
            catch (Exception ex)
            {
                _log!.Error($"[HeadsetViewModel] DTH UpdateDTPValue ...... {ex.ToString()}");
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

        private string _regKeyForQRCode = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.HeadsetQRCode.";
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

        private async Task DoWork_PleaseWait(string model, HeadsetViewModel vm)
        {
            _log.Info($"[HeadsetViewModel] DoWork_PleaseWait .......");
            // Simulate time-consuming operation
            Thread.Sleep(500);
            if (IsDTPReady)
                UpdateDTPValue();
            else
                UpdateDTHValue();
            // Call DetectPageShow
            DetectPageShow(model);
            //HidePleaseWait();
        }

        public async Task Invoke_PleaseWaitAsync(string model, HeadsetViewModel vm)
        {
            vm.ShowPleaseWait();
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    await Task.Run(() => DoWork_PleaseWait(model, vm), cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                _log!.Error("[HeadsetViewModel] Invoke_PleaseWaitAsync timed out");
                throw;
            }
            catch (Exception ex)
            {
                vm._log!.Error($"[HeadsetViewModel] Invoke_PleaseWaitAsync exception: {ex.Message}");
                throw;
            }
            finally
            {
                vm.HidePleaseWait();
            }
        }

        private void RunWorkerCompleted_PleaseWait(object sender, RunWorkerCompletedEventArgs e)
        {
            HidePleaseWait();
        }

        #endregion Please Wait

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
        #region HeadsetAudioSettings ToggleSwitch Binding

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
                _isOutgoingAudioStatus = DeviceInfoDTP!.MicNoiseCancellation;
                return _isOutgoingAudioStatus;
            }
            set
            {
                _isRestoreEnable = false;
                DeviceInfoDTP!.MicNoiseCancellation = value;
                _isOutgoingAudioStatus = value;
                _supportedOutgoingAudio = false;
                _deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.MicNoiseCancellation).Wait();
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
                _isIncomingAudioStatus = DeviceInfoDTP!.MicNCIncoming;
                return _isIncomingAudioStatus;
            }
            set
            {
                _isRestoreEnable = false;
                DeviceInfoDTP!.MicNCIncoming = value;
                _isIncomingAudioStatus = value;
                _supportedIncomingAudio = false;
                if (IsDTPReady)
                    _deviceManager.SetMicNCIncomingAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.MicNCIncoming).Wait();
                else
                    _deviceManager.SetMicNCIncoming(value, CurrentDeviceInfo!.ID).Wait();
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
                return _supportedMicNoiseCancellation;
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
                _isMicNoiseCancellationStatus = DeviceInfoDTP!.MicNoiseCancellation;
                return _isMicNoiseCancellationStatus;
            }
            set
            {
                _isRestoreEnable = false;
                DeviceInfoDTP!.MicNoiseCancellation = value;
                _isMicNoiseCancellationStatus = value;
                _supportedMicNoiseCancellation = false;
                if (IsDTPReady)
                    _deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.MicNoiseCancellation).Wait();
                else
                    _deviceManager.SetMicNoiseCancellation(value, CurrentDeviceInfo!.ID).Wait();
                //_debouncerHeadset.Debounce("MicNoiseCancellationCheck");
                _supportedMicNoiseCancellation = true;
                OnPropertyChanged(nameof(MicNoiseCancellation_String));
                _log.Info($"[HeadsetViewModel] SetMicNoiseCancellationAsync ... MicNoiseCancellationCheck .... {DeviceInfoDTP.MicNoiseCancellation.ToString()}");
            }
        }

        //Sidetone ToggleSwitch
        private string _isSidetone_String = "ON";

        public string Sidetone_String
        {
            get => _isSidetoneStatus ? Strings.On : Strings.Off;
        }

        private bool _isSidetoneStatus;// = false;

        public bool SidetoneStatus
        {
            get
            {
                _isSidetoneStatus = DeviceInfoDTP!.Sidetone;
                return _isSidetoneStatus;
            }
            set
            {
                _isRestoreEnable = false;
                DeviceInfoDTP!.Sidetone = value;
                _isSidetoneStatus = value;
                if (IsDTPReady)
                {
                    _debouncerHeadsetSidetoneCheck.Debounce("SidetoneCheck");
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
                _isidetoneSliderValue = DeviceInfoDTP.SidetoneLevel;//_deviceManager.GetSidetoneLevelAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.SidetoneLevel;
                return _isidetoneSliderValue;
            }

            set
            {
                _isRestoreEnable = false;
                DeviceInfoDTP!.SidetoneLevel = value;
                _isidetoneSliderValue = value;
                if (IsDTPReady)
                    _debouncerHeadset.Debounce("SidetoneSlider");
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

        #endregion HeadsetAudioSettings ToggleSwitch Binding

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

        #endregion HeadsetAudioSettings Grid Show/Hide

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
                        DeviceInfoDTP.AncMode = 1;
                        _isTransparencyChecked = false;
                        _isNoiseOffChecked = false;
                        DeviceInfoDTP!.Sidetone = true;
                        _isSidetoneStatus = true;
                        if (IsDTPReady)
                        {
                            _debouncerHeadset.Debounce("ANC");
                            //_debouncerHeadsetSidetoneCheck.Debounce("SidetoneCheck");
                        }
                        else
                        {
                            _deviceManager.SetAncMode(1, CurrentDeviceInfo!.ID).Wait();
                            //_deviceManager.SetSidetone(value, CurrentDeviceInfo!.ID).Wait();
                        }
                        //SidetoneStatus = true;
                        _isSidetoneStatus = true;
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
                        DeviceInfoDTP.AncMode = 2;
                        _isActiveNoiseCancellingChecked = false;
                        _isNoiseOffChecked = false;
                        DeviceInfoDTP!.Sidetone = false;
                        //SidetoneStatus = false;
                        _isSidetoneStatus = false;
                        if (IsDTPReady)
                        {                         
                            _debouncerHeadset.Debounce("Transparency");
                            //if (IsDTPReady)
                            //    _debouncerHeadsetSidetoneCheck.Debounce("SidetoneCheck");
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
                        DeviceInfoDTP.AncMode = 0;
                        _isActiveNoiseCancellingChecked = false;
                        _isTransparencyChecked = false;
                        DeviceInfoDTP!.Sidetone = true;
                        //SidetoneStatus = true;
                        _isSidetoneStatus = true;
                        if (IsDTPReady)
                        {
                            _debouncerHeadset.Debounce("NoiseOff");
                            //_debouncerHeadsetSidetoneCheck.Debounce("SidetoneCheck");
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
                _isTransparencylevelSliderValue = DeviceInfoDTP.AncGain;//_deviceManager.GetAncGainAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.AncGain;
                return _isTransparencylevelSliderValue;
            }

            set
            {
                _isRestoreEnable = false;
                DeviceInfoDTP.AncGain = value;
                _isTransparencylevelSliderValue = value;
                if (IsDTPReady)
                    _debouncerHeadset.Debounce("TransparencylevelSlider");
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
                    if (_isDefaultChecked)
                    {
                        _isRestoreEnable = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        if (IsDTPReady)
                            DeviceInfoDTP.SelectedPreset = 1;
                        else
                            _deviceManager.SetSelectedPreset(1, CurrentDeviceInfo!.ID).Wait();
                        _debouncerHeadset.Debounce("DefaultCheck");
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
                    _isBassBoostChecked = value;
                    if (_isBassBoostChecked)
                    {
                        _isRestoreEnable = false;
                        _isDefaultChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        DeviceInfoDTP.SelectedPreset = 3;
                        if (IsDTPReady)
                            _debouncerHeadset.Debounce("BassBoostCheck");
                        else
                            _deviceManager.SetSelectedPreset(3, CurrentDeviceInfo!.ID).Wait();
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
                    _isSpeechBoostChecked = value;
                    if (_isSpeechBoostChecked)
                    {
                        _isRestoreEnable = false;
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        DeviceInfoDTP.SelectedPreset = 2;
                        if (IsDTPReady)
                            _debouncerHeadset.Debounce("SpeechBoostCheck");
                        else
                            _deviceManager.SetSelectedPreset(2, CurrentDeviceInfo!.ID).Wait();
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
                    _isTrebleBoostChecked = value;
                    if (_isTrebleBoostChecked)
                    {
                        _isRestoreEnable = false;
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isCustomChecked = false;
                        DeviceInfoDTP.SelectedPreset = 4;
                        if (IsDTPReady)
                            _debouncerHeadset.Debounce("TrebleBoostCheck");
                        else
                            _deviceManager.SetSelectedPreset(4, CurrentDeviceInfo!.ID).Wait();
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
                    _isCustomChecked = value;
                    if (_isCustomChecked)
                    {
                        _isRestoreEnable = false;
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _audioEqualizerGridPageShow = true;
                        DeviceInfoDTP.SelectedPreset = 101;
                        if (IsDTPReady)
                            _debouncerHeadset.Debounce("CustomCheck");
                        else
                            _deviceManager.SetSelectedPreset(101, CurrentDeviceInfo!.ID).Wait();
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

        #endregion HeadsetAudioSettingsRightView

        #region HeadsetAudioSettingsRightViewToolTip

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

        #endregion HeadsetAudioSettingsRightViewToolTip

        /// <summary>
        /// HeadsetAutomatedActions Page
        /// </summary>

        #region HeadsetAutomatedActions ToggleSwitch Binding

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
                DeviceInfoDTP.WearDetectionFromDTP = value;
                //_supportedWearDetection = false;
                _debouncerHeadset.Debounce("WearDetectionCheck");
                //_deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.WearDetectionFromDTP).Wait();
                //_supportedWearDetection = true;
                OnPropertyChanged(nameof(WearDetection_String));
                _log.Info($"[HeadsetViewModel] SetWearDetectionAsync ... WearDetectionCheck ... {DeviceInfoDTP.WearDetectionFromDTP.ToString()}");
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
                    DeviceInfoDTP.WearDetectionFromDTP = false;
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.WearDetectionFromDTP).Wait();
                    OnPropertyChanged(nameof(WearDetectionStatus));
                    OnPropertyChanged(nameof(WearDetection_String));
                }
                _isPauseMusicStatus = value;
                DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP = value;
                _debouncerHeadsetPauseMusic.Debounce("PauseMusicCheck");
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
                    DeviceInfoDTP.WearDetectionFromDTP = false;
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.WearDetectionFromDTP).Wait();
                    OnPropertyChanged(nameof(WearDetectionStatus));
                    OnPropertyChanged(nameof(WearDetection_String));
                }
                _isMuteMicrophoneStatus = value;
                DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP = value;
                _debouncerHeadsetMuteMicrophone.Debounce("MuteMicrophoneCheck");
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
                    DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 0;
                else
                    DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = 1;

                //_debouncerHeadsetQuickPause.Debounce("QuickPauseCheck");
                _debouncerHeadset.Debounce("QuickPauseCheck");
                OnPropertyChanged(nameof(QuickPause_String));
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
                _isAnswerCallsStatus = DeviceInfoDTP.AnswerCall;
                return _isAnswerCallsStatus;
            }
            set
            {
                _isRestoreEnable = false;
                _isAnswerCallsStatus = value;
                DeviceInfoDTP.AnswerCall = value;
                _supportedAnswerCalls = false;
                _deviceManager.SetBoomMicAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.AnswerCall).Wait();
                _supportedAnswerCalls = true;
                //_debouncerHeadset.Debounce("AnswerCallsCheck");
                OnPropertyChanged(nameof(AnswerCalls_String));
                _log.Info($"[HeadsetViewModel] SetBoomMicAsync ... AnswerCallsCheck .... {DeviceInfoDTP.AnswerCall.ToString()}");
            }
        }

        #endregion HeadsetAutomatedActions ToggleSwitch Binding

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

        #endregion HeadsetAutomatedActions Grid Show/Hide

        #region HeadsetAutomatedActionsRightView

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
                        DeviceInfoDTP.WearDetectionSensitivityFromDTP = Convert.ToInt32(value);
                        _debouncerHeadset.Debounce("Normal2Check");
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
                        DeviceInfoDTP.WearDetectionSensitivityFromDTP = Convert.ToInt32(!value);
                        _debouncerHeadset.Debounce("LowCheck");
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
                        DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = setWear;
                        _debouncerHeadset.Debounce("NormalCheck");
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
                        DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP = setWear;
                        _debouncerHeadset.Debounce("SensitiveCheck");
                        OnPropertyChanged(nameof(IsNormalChecked));
                        OnPropertyChanged(nameof(IsSensitiveChecked));
                    }
                }
            }
        }

        #endregion HeadsetAutomatedActionsRightView

        #region HeadsetAutomatedActionsToolTip

        private string _wearDetectionToolTip = Strings.HeadsetAutomatedActionsToolTip_1;//"Automatic actions when you remove your headset";

        public string WearDetectionToolTip
        {
            get => _wearDetectionToolTip;
        }

        private string _pauseMusicToolTip = Strings.HeadsetAutomatedActionsToolTip_2;//"Pauses music automatically when headset is removed. Music will resume automatically when headset is put on.";

        public string PauseMusicToolTip
        {
            get => _pauseMusicToolTip;
        }

        private string _muteMicrophoneToolTip = Strings.HeadsetAutomatedActionsToolTip_3;//"Mutes microphone automatically when headset is removed";

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

        #endregion HeadsetAutomatedActionsToolTip

        /// <summary>
        /// HeadsetDeviceSettings Page
        /// </summary>

        #region HeadsetDeviceSettings ToggleSwitch Binding

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
                _isBusyLightStatus = DeviceInfoDTP.BusyLight;//_deviceManager.GetBusyLightAsync(CurrentDeviceInfo!.ID.ToString()).Result; //CurrentDeviceInfo!.BusyLight;
                return _isBusyLightStatus;
            }
            set
            {
                _isRestoreEnable = false;
                _isBusyLightStatus = value;
                DeviceInfoDTP.BusyLight = value;
                _supportedBusyLight = false;
                if (IsDTPReady)
                    _deviceManager.SetBusyLightAsync(CurrentDeviceInfo!.ID.ToString(), DeviceInfoDTP.BusyLight).Wait();
                else
                    _deviceManager.SetBusyLight(value, CurrentDeviceInfo!.ID).Wait();
                _supportedBusyLight = true;
                //_debouncerHeadset.Debounce("BusyLightCheck");
                OnPropertyChanged("BusyLight_String");
                _log.Info($"[HeadsetViewModel] SetBusyLightAsync ....... {DeviceInfoDTP.BusyLight.ToString()}");
            }
        }

        #endregion HeadsetDeviceSettings ToggleSwitch Binding

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

        #endregion HeadsetDeviceSettings Grid Show/Hide

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
                        DeviceInfoDTP.VoiceGuidance = false;
                        if (IsDTPReady)
                            _debouncerHeadset.Debounce("EssentialCheck");
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
                        DeviceInfoDTP.VoiceGuidance = true;
                        if (IsDTPReady)
                            _debouncerHeadset.Debounce("AllCheck");
                        else
                            _deviceManager.SetVoiceGuidance(true, CurrentDeviceInfo!.ID).Wait();
                        OnPropertyChanged(nameof(IsEssentialChecked));
                    }
                }
            }
        }

        #endregion HeadsetDeviceSettingsRightView

        #region HeadsetDeviceSettingsToolTip

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

        #endregion HeadsetDeviceSettingsToolTip

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

        private bool CheckIfCurrentSettingsMatchDefault(DeviceInfoDTP currentSettings, string model)
        {
            if (!ModelDefaultSettings.ContainsKey(model))
            {
                return true;
            }

            var defaultSettings = ModelDefaultSettings[model];
            if (currentSettings.AncMode != defaultSettings.AncMode) return false;
            if (currentSettings.AncGain != defaultSettings.AncGain) return false;
            if (currentSettings.BusyLight != defaultSettings.BusyLight) return false;
            if (currentSettings.MicNoiseCancellation != defaultSettings.MicNoiseCancellation) return false;
            if (currentSettings.Sidetone != defaultSettings.Sidetone) return false;
            if (currentSettings.VoiceGuidance != defaultSettings.VoiceGuidance) return false;
            if (currentSettings.SelectedPreset != defaultSettings.SelectedPreset) return false;
            if (currentSettings.Band1Gain != defaultSettings.Band1Gain) return false;
            if (currentSettings.Band2Gain != defaultSettings.Band2Gain) return false;
            if (currentSettings.Band3Gain != defaultSettings.Band3Gain) return false;
            if (currentSettings.Band4Gain != defaultSettings.Band4Gain) return false;
            if (currentSettings.Band5Gain != defaultSettings.Band5Gain) return false;
            if (currentSettings.MicNCIncoming != defaultSettings.MicNCIncoming) return false;
            if (currentSettings.WearDetectionFromDTP != defaultSettings.WearDetectionFromDTP) return false;
            if (currentSettings.IsWearDetectionPauseMusicEnableFromDTP != defaultSettings.IsWearDetectionPauseMusicEnableFromDTP) return false;
            if (currentSettings.IsWearDetectionMuteMicEnabledFromDTP != defaultSettings.IsWearDetectionMuteMicEnabledFromDTP) return false;
            if (currentSettings.WearDetectionQuickPauseAsyncFromDTP != defaultSettings.WearDetectionQuickPauseAsyncFromDTP) return false;
            if (currentSettings.WearDetectionSensitivityFromDTP != defaultSettings.WearDetectionSensitivityFromDTP) return false;
            if (currentSettings.AnswerCall != defaultSettings.AnswerCall) return false;
            return true;
        }
        private static readonly Dictionary<string, HeadsetDeviceDefaultSettings> ModelDefaultSettings =
        new Dictionary<string, HeadsetDeviceDefaultSettings>()
        {
            { "WL7024", new HeadsetDeviceDefaultSettings {
                AncMode = 1,
                AncGain = 3,
                BusyLight = true,
                MicNoiseCancellation = true,
                Sidetone = true,
                VoiceGuidance = true,
                SelectedPreset = 1,
                Band1Gain = 0,
                Band2Gain = 0,
                Band3Gain = 0,
                Band4Gain = 0,
                Band5Gain = 0,
                MicNCIncoming = false,
                WearDetectionFromDTP = true,
                IsWearDetectionPauseMusicEnableFromDTP = true,
                IsWearDetectionMuteMicEnabledFromDTP = true,
                WearDetectionQuickPauseAsyncFromDTP = 2,
                WearDetectionSensitivityFromDTP = 0,
                AnswerCall = false
            }},
            { "WH3024", new HeadsetDeviceDefaultSettings {
                AncMode = 0,
                AncGain = 0,
                BusyLight = true,
                MicNoiseCancellation = true,
                Sidetone = true,
                VoiceGuidance = false,
                SelectedPreset = 1,
                Band1Gain = 0,
                Band2Gain = 0,
                Band3Gain = 0,
                Band4Gain = 0,
                Band5Gain = 0,
                MicNCIncoming = false,
                WearDetectionFromDTP = false,
                IsWearDetectionPauseMusicEnableFromDTP = false,
                IsWearDetectionMuteMicEnabledFromDTP = false,
                WearDetectionQuickPauseAsyncFromDTP = 0,
                WearDetectionSensitivityFromDTP = 0,
                AnswerCall = false
            }},
            { "WL3024", new HeadsetDeviceDefaultSettings {
                AncMode = 0,
                AncGain = 0,
                BusyLight = true,
                MicNoiseCancellation = true,
                Sidetone = true,
                VoiceGuidance = true,
                SelectedPreset = 1,
                Band1Gain = 0,
                Band2Gain = 0,
                Band3Gain = 0,
                Band4Gain = 0,
                Band5Gain = 0,
                MicNCIncoming = false,
                WearDetectionFromDTP = false,
                IsWearDetectionPauseMusicEnableFromDTP = false,
                IsWearDetectionMuteMicEnabledFromDTP = false,
                WearDetectionQuickPauseAsyncFromDTP = 0,
                WearDetectionSensitivityFromDTP = 0,
                AnswerCall = false
            }},
            { "WL5024", new HeadsetDeviceDefaultSettings {
                AncMode = 1,
                AncGain = 3,
                BusyLight = true,
                MicNoiseCancellation = true,
                Sidetone = true,
                VoiceGuidance = true,
                SelectedPreset = 1,
                Band1Gain = 0,
                Band2Gain = 0,
                Band3Gain = 0,
                Band4Gain = 0,
                Band5Gain = 0,
                MicNCIncoming = false,
                WearDetectionFromDTP = true,
                IsWearDetectionPauseMusicEnableFromDTP = true,
                IsWearDetectionMuteMicEnabledFromDTP = true,
                WearDetectionQuickPauseAsyncFromDTP = 0,
                WearDetectionSensitivityFromDTP = 1,
                AnswerCall = false
            }},
            { "WH5024", new HeadsetDeviceDefaultSettings {
                AncMode = 1,
                AncGain = 3,
                BusyLight = true,
                MicNoiseCancellation = true,
                Sidetone = true,
                VoiceGuidance = true,
                SelectedPreset = 1,
                Band1Gain = 0,
                Band2Gain = 0,
                Band3Gain = 0,
                Band4Gain = 0,
                Band5Gain = 0,
                MicNCIncoming = false,
                WearDetectionFromDTP = false,
                IsWearDetectionPauseMusicEnableFromDTP = false,
                IsWearDetectionMuteMicEnabledFromDTP = false,
                WearDetectionQuickPauseAsyncFromDTP = 0,
                WearDetectionSensitivityFromDTP = 0,
                AnswerCall = false
            }}
        };
    }

    /// <summary>
    /// DeviceInfo + DTP
    /// </summary>
    #region DeviceInfo  DTP

    public class DeviceInfoDTP : DeviceInfo
    {
        private bool _isAnswerCallSupported;
        private bool _isAnswerCall;

        public bool AnswerCall
        {
            get => _isAnswerCall;
            set
            {
                _isAnswerCall = value;
                OnPropertyChanged();
            }
        }

        public bool IsAnswerCallSupported
        {
            get => _isAnswerCallSupported;
            set
            {
                _isAnswerCallSupported = value;
                OnPropertyChanged();
            }
        }

        private bool _wearDetectionFromDTP;
        private bool _isWearDetectionMuteMicEnabledFromDTP;
        private bool _isWearDetectionPauseMusicEnableFromDTP;
        private int _wearDetectionQuickPauseAsyncFromDTP;
        private int _wearDetectionSensitivityFromDTP;

        public bool WearDetectionFromDTP
        {
            get => _wearDetectionFromDTP;
            set
            {
                _wearDetectionFromDTP = value;
                OnPropertyChanged();
            }
        }

        public bool IsWearDetectionMuteMicEnabledFromDTP
        {
            get => _isWearDetectionMuteMicEnabledFromDTP;
            set
            {
                _isWearDetectionMuteMicEnabledFromDTP = value;
                OnPropertyChanged();
            }
        }

        public bool IsWearDetectionPauseMusicEnableFromDTP
        {
            get => _isWearDetectionPauseMusicEnableFromDTP;
            set
            {
                _isWearDetectionPauseMusicEnableFromDTP = value;
                OnPropertyChanged();
            }
        }

        public int WearDetectionQuickPauseAsyncFromDTP
        {
            get => _wearDetectionQuickPauseAsyncFromDTP;
            set
            {
                _wearDetectionQuickPauseAsyncFromDTP = value;
                OnPropertyChanged();
            }
        }

        public int WearDetectionSensitivityFromDTP
        {
            get => _wearDetectionSensitivityFromDTP;
            set
            {
                _wearDetectionSensitivityFromDTP = value;
                OnPropertyChanged();
            }
        }
    }
    public class HeadsetDeviceDefaultSettings
    {
        public int AncMode { get; set; } = 0;
        public int AncGain { get; set; } = 0;
        public bool BusyLight { get; set; } = false;
        public bool MicNoiseCancellation { get; set; } = false;
        public bool Sidetone { get; set; } = false;
        public bool VoiceGuidance { get; set; } = false;
        public int SelectedPreset { get; set; } = 1;
        public int Band1Gain { get; set; } = 0;
        public int Band2Gain { get; set; } = 0;
        public int Band3Gain { get; set; } = 0;
        public int Band4Gain { get; set; } = 0;
        public int Band5Gain { get; set; } = 0;
        public bool MicNCIncoming { get; set; } = false;
        public bool WearDetectionFromDTP { get; set; } = false;
        public bool IsWearDetectionPauseMusicEnableFromDTP { get; set; } = false;
        public bool IsWearDetectionMuteMicEnabledFromDTP { get; set; } = false;
        public int WearDetectionQuickPauseAsyncFromDTP { get; set; } = 0;
        public int WearDetectionSensitivityFromDTP { get; set; } = 0;
        public bool AnswerCall { get; set; } = false;
    }

    //***********************************************************************
    //DeviceInfoDTP.AncMode.....................= 2
    //DeviceInfoDTP.AncGain.....................= 3
    //DeviceInfoDTP.BusyLight...................= True
    //DeviceInfoDTP.MicNoiseCancellation.............= True
    //DeviceInfoDTP.Sidetone...................= False
    //DeviceInfoDTP.VoiceGuidance..............= True
    //DeviceInfoDTP.SelectedPreset.............= 1
    //DeviceInfoDTP.Band1Gain..................= 0
    //DeviceInfoDTP.Band2Gain..................= 0
    //DeviceInfoDTP.Band3Gain..................= 0
    //DeviceInfoDTP.Band4Gain..................= 0
    //DeviceInfoDTP.Band5Gain..................= 0
    //DeviceInfoDTP.MicNCIncoming.............= False
    //DeviceInfoDTP.GetIsWearDetectionSupportedAsync.............= True
    //DeviceInfoDTP.WearDetectionFromDTP.............= True
    //DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.............= True
    //DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP.............= True
    //DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP.............= True
    //DeviceInfoDTP.WearDetectionSensitivityFromDTP.............= True
    //GetIsBoomMicSupportedAsync.............NO
    //DetectPageShow...WL7024


    //GetIsANCSupportedAsync..............................NO
    //DeviceInfoDTP.BusyLight...................= True
    //DeviceInfoDTP.MicNoiseCancellation.............= True
    //DeviceInfoDTP.Sidetone...................= True
    //GetIsVoiceGuidanceSupportedAsync....................NO
    //DeviceInfoDTP.SelectedPreset.............= 1
    //DeviceInfoDTP.Band1Gain..................= 0
    //DeviceInfoDTP.Band2Gain..................= 0
    //DeviceInfoDTP.Band3Gain..................= 0
    //DeviceInfoDTP.Band4Gain..................= 0
    //DeviceInfoDTP.Band5Gain..................= 0
    //GetIsMicNCIncomingSupportedAsync.............NO
    //GetIsWearDetectionSupportedAsync.............NO
    //DeviceInfoDTP.AnswerCall.............= False
    //DetectPageShow...WH3024

    //GetIsANCSupportedAsync..............................NO
    //DeviceInfoDTP.BusyLight...................= True
    //DeviceInfoDTP.MicNoiseCancellation.............= True
    //DeviceInfoDTP.Sidetone...................= True
    //DeviceInfoDTP.VoiceGuidance..............= True
    //DeviceInfoDTP.SelectedPreset.............= 1
    //DeviceInfoDTP.Band1Gain..................= 0
    //DeviceInfoDTP.Band2Gain..................= 0
    //DeviceInfoDTP.Band3Gain..................= 0
    //DeviceInfoDTP.Band4Gain..................= 0
    //DeviceInfoDTP.Band5Gain..................= 0
    //GetIsMicNCIncomingSupportedAsync.............NO
    //GetIsWearDetectionSupportedAsync.............NO
    //DeviceInfoDTP.AnswerCall.............= False
    //DetectPageShow...WL3024


    //DeviceInfoDTP.AncMode.....................= 0
    //DeviceInfoDTP.AncGain.....................= 3
    //DeviceInfoDTP.BusyLight...................= True
    //DeviceInfoDTP.MicNoiseCancellation.............= True
    //DeviceInfoDTP.Sidetone...................= True
    //DeviceInfoDTP.VoiceGuidance..............= True
    //DeviceInfoDTP.SelectedPreset.............= 1
    //DeviceInfoDTP.Band1Gain..................= 0
    //DeviceInfoDTP.Band2Gain..................= 0
    //DeviceInfoDTP.Band3Gain..................= 0
    //DeviceInfoDTP.Band4Gain..................= 0
    //DeviceInfoDTP.Band5Gain..................= 0
    //GetIsMicNCIncomingSupportedAsync.............NO
    //DeviceInfoDTP.GetIsWearDetectionSupportedAsync.............= True
    //DeviceInfoDTP.WearDetectionFromDTP.............= True
    //DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.............= True
    //DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP.............= True
    //DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP.............= True
    //DeviceInfoDTP.WearDetectionSensitivityFromDTP.............= True
    //DeviceInfoDTP.AnswerCall.............= False
    //DetectPageShow...WL5024



    //DeviceInfoDTP.AncMode.....................= 1
    //DeviceInfoDTP.AncGain.....................= 3
    //DeviceInfoDTP.BusyLight...................= True
    //DeviceInfoDTP.MicNoiseCancellation.............= True
    //DeviceInfoDTP.Sidetone...................= True
    //DeviceInfoDTP.VoiceGuidance..............= True
    //DeviceInfoDTP.SelectedPreset.............= 1
    //DeviceInfoDTP.Band1Gain..................= 0
    //DeviceInfoDTP.Band2Gain..................= 0
    //DeviceInfoDTP.Band3Gain..................= 0
    //DeviceInfoDTP.Band4Gain..................= 0
    //DeviceInfoDTP.Band5Gain..................= 0
    //GetIsMicNCIncomingSupportedAsync.............NO
    //GetIsWearDetectionSupportedAsync.............NO
    //DeviceInfoDTP.AnswerCall.............= False
    //DetectPageShow...WH5024
    #endregion
}