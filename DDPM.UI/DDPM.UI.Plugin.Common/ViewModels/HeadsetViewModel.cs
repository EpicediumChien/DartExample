using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DDPM.UI.Plugin.ViewModels
{
    public class HeadsetViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables

        public readonly ILog _log;
        public IDeviceManagerSA _deviceManager;
        public DeviceInfo DeviceInfoDTP;
        public string _current_headset;
        public readonly string regPath = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Global\QRCode";
        public readonly string regKeyForQRCode = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.HeadsetQRCode";
        #endregion Variables

        public new event PropertyChangedEventHandler? PropertyChanged;
        //public string modelTest;
        public HeadsetViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;

            DeviceInfoDTP = new DeviceInfo();
            _current_headset = string.Empty;
            _log!.Info($"[HeadsetViewModel] HeadsetViewModel Start...");
        }

        public void DetectPageShow(string model)
        {
            //modelTest = model;
            ReadQRCodeReg();
            AllResetHeadsetPage();

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
                    _automatedActionsAnswerCallPageShow = false;//DELL 說拿掉
                    //Page 3
                    _voiceGuidancePageShow = true;
                    //_deviceSettingsDownloadDellAudioPageShow = false;
                    break;

                case "WH5024"://Winflo
                    //Page 1
                    _controlTheNoiseIHearPageShow = true;
                    _configureMyAudioModesPageShow = true;
                    //Page 2
                    _automatedActionsAnswerCallPageShow = false;//DELL 說拿掉;
                    //Page 3
                    _voiceGuidancePageShow = true;
                    break;

                case "WL3024"://Vaporify
                    //Page 1
                    _configureMyAudioModesPageShow = true;
                    //Page 2
                    _automatedActionsAnswerCallPageShow = false;//DELL 說拿掉;
                    //Page 3
                    _voiceGuidancePageShow = true;
                    //_deviceSettingsDownloadDellAudioPageShow = false;
                    break;

                case "WH3024"://Airmax
                    //Page 1
                    _controlTheNoiseIHearPageShow = false;//Fix PIMS-PIMS-294568
                    _configureMyAudioModesPageShow = true;
                    //Page 2
                    _automatedActionsAnswerCallPageShow = false;//DELL 說拿掉;
                    //Page 3
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
                    regValue = DdpmCommonHelper.DeviceManagerSA!.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKeyForQRCode).Result;

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
            CheckMicNCIncomingUI(false);
            CheckMicNoiseCancellationUI(false);
            CheckWearDetectionUI(false);
            CheckPresetsUI(false);
            CheckVoiceGuidanceUI(false);
            CheckANCUI(false);
        }

        private void CheckSidetoneUI(bool PropertyChange)
        {
            if (DeviceInfoDTP!.IsSidetoneSupported)
            {
                _isSidetoneStatus = DeviceInfoDTP.Sidetone;//_deviceManager.GetSidetoneAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo.Sidetone;

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
            if (DeviceInfoDTP!.IsSidetoneSupported)
            {
                int sidevalue = DeviceInfoDTP.SidetoneLevel;//_deviceManager.GetSidetoneLevelAsync(CurrentDeviceInfo!.ID.ToString()).Result;
                if (_isidetoneSliderValue != sidevalue)//CurrentDeviceInfo.SidetoneLevel)
                {
                    _isidetoneSliderValue = sidevalue;

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
            if (DeviceInfoDTP!.IsBusyLightSupported)
                _isBusyLightStatus = DeviceInfoDTP.BusyLight;//_deviceManager.GetBusyLightAsync(CurrentDeviceInfo!.ID.ToString()).Result; //CurrentDeviceInfo.BusyLight;
            if (PropertyChange)
            {
                OnPropertyChanged("BusyLightStatus");
                OnPropertyChanged("BusyLight_String");
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
                        OnPropertyChanged("IncomingAudioStatus");
                        OnPropertyChanged("IncomingAudio_String");
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
                OnPropertyChanged("MicNoiseCancellationStatus");
                OnPropertyChanged("MicNoiseCancellation_String");
                UpdateCollaborationAndultimediaUI(true, false);
            }
        }

        private void CheckWearDetectionUI(bool PropertyChange)
        {
            if (DeviceInfoDTP!.IsWearDetectionSupported)
            {
                uint wearDetectionValue = (uint)DeviceInfoDTP!.WearDetection;
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
                OnPropertyChanged("IsAllChecked");
                OnPropertyChanged("IsEssentialChecked");
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
            _log.Info($"[HeadsetViewModel] SetCurrentDevice ...");
            //deviceID ??= DeviceInfos.Values.ToList().FirstOrDefault()!.ID.ToString();
            if (!base.SetCurrentDevice(deviceID))
                return false;
            _log.Info($"[HeadsetViewModel] SetCurrentDevice GUID ... {CurrentDeviceID.ToString()}");
            _current_headset = CurrentDeviceID!.ToString();
            var fv = _deviceManager.GetFirmwareVersionAsync(CurrentDeviceID.ToString()).Result; //CurrentDeviceInfo.FirmwareVersion.PadLeft(4, '0');
            //FirmwareVersion2 = $"Firmware Version {fv}";// {fv.Substring(0, 1)}.{fv.Substring(1, 1)}.{fv.Substring(2, 1)}.{fv.Substring(3, 1)}";
            FirmwareVersion2 = Strings.FirmwareVersion + $" {fv}";
            //else
            //    _current_headset = deviceID;
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
                        //GenerateInfo();
                    }
                    break;

                default:
                    break;
            }
        }
        public void RestoreToDefault()
        {          
            _log.Info($"[HeadsetViewModel] Print before property ...RestoreToDefault ... in");
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
            _deviceManager.SetFactoryResetAsyncValueForHeadset(CurrentDeviceInfo!.ID.ToString(), true).Wait();
            UpdateDTPValue();
            CheckHeadsetFunc();
        }
        private void UpdateDTPValue()
        {
            _log.Info($"[HeadsetViewModel] Print before property ...UpdateDTPValue ... in");
            if (DeviceInfoDTP == null)
            {
                DeviceInfoDTP = new DeviceInfo();
            }

            DeviceInfoDTP.AncGain = _deviceManager.GetAncGainAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.AncMode = _deviceManager.GetAncModeAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.BatteryLevel = _deviceManager.GetBatteryLevelAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.BusyLight = _deviceManager.GetBusyLightAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.MicNoiseCancellation = _deviceManager.GetMicNoiseCancellationAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.MicNCIncoming = _deviceManager.GetMicNCIncomingAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.Sidetone = _deviceManager.GetSidetoneAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.SidetoneLevel = _deviceManager.GetSidetoneLevelAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.SelectedPreset = _deviceManager.GetSelectedPresetAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.VoiceGuidance = _deviceManager.GetVoiceGuidanceAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.WearDetection = _deviceManager.GetWearDetectionAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.IsANCSupported = _deviceManager.GetIsANCSupportedAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.IsBusyLightSupported = _deviceManager.GetIsBusyLightSupportedAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.IsMicNoiseCancellationSupported = _deviceManager.GetIsMicNoiseCancellationSupportedAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.IsSidetoneSupported = _deviceManager.GetIsSidetoneSupportedAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.IsVoiceGuidanceSupported = _deviceManager.GetIsVoiceGuidanceSupportedAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.IsPresetsSupported = _deviceManager.GetIsPresetsSupportedAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.IsMicNCIncomingSupported = _deviceManager.GetIsMicNCIncomingSupportedAsync(CurrentDeviceID.ToString()).Result;
            DeviceInfoDTP.IsWearDetectionSupported = _deviceManager.GetIsWearDetectionSupportedAsync(CurrentDeviceID.ToString()).Result;
            //DeviceInfoDTP.BandsGain = _deviceManager.GetBandsGainAsync(CurrentDeviceID.ToString()).Result;
            _log.Info($"[HeadsetViewModel] Print after property ......");
            _log.Info($"[HeadsetViewModel] ***********************************************************************");
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
            _log.Info($"[HeadsetViewModel] DoWork_PleaseWait .......");
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
                _isOutgoingAudioStatus = DeviceInfoDTP!.MicNoiseCancellation;
                return _isOutgoingAudioStatus;
            }
            set
            {
                _log.Info($"[HeadsetViewModel] SetMicNoiseCancellationAsync For OutgoingAudioStatus ....... {value.ToString()}");
                _deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(), value).Wait();
                DeviceInfoDTP!.MicNoiseCancellation = value;
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
                _isIncomingAudioStatus = DeviceInfoDTP!.MicNCIncoming;
                return _isIncomingAudioStatus;
            }
            set
            {
                _log.Info($"[HeadsetViewModel] SetMicNCIncomingAsync ....... {value.ToString()}");
                //_deviceManager.SetMicNCIncoming(value, CurrentDeviceInfo!.ID).Wait();
                _deviceManager.SetMicNCIncomingAsync(CurrentDeviceInfo!.ID.ToString(), value).Wait();
                DeviceInfoDTP!.MicNCIncoming = value;
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
                _isMicNoiseCancellationStatus = DeviceInfoDTP!.MicNoiseCancellation;
                return _isMicNoiseCancellationStatus;
            }
            set
            {
                _log.Info($"[HeadsetViewModel] SetMicNoiseCancellationAsync ....... {value.ToString()}");
                //_deviceManager.SetMicNoiseCancellation(value, CurrentDeviceInfo!.ID).Wait();
                _deviceManager.SetMicNoiseCancellationAsync(CurrentDeviceInfo!.ID.ToString(),value).Wait();
                DeviceInfoDTP!.MicNoiseCancellation = value;
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
                _isSidetoneStatus = DeviceInfoDTP!.Sidetone;
                return _isSidetoneStatus;
            }
            set
            {
                _log.Info($"[HeadsetViewModel] SetSidetoneAsync ....... {value.ToString()}");
                //_deviceManager.SetSidetone(value, CurrentDeviceInfo!.ID).Wait();
                _deviceManager.SetSidetoneAsync(CurrentDeviceInfo!.ID.ToString(), value).Wait();
                DeviceInfoDTP!.Sidetone = value;
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
                _isidetoneSliderValue = DeviceInfoDTP.SidetoneLevel;//_deviceManager.GetSidetoneLevelAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.SidetoneLevel;
                return _isidetoneSliderValue;
            }

            set
            {
                _log.Info($"[HeadsetViewModel] SetSidetoneLevelAsync ....... {value.ToString()}");
                //_deviceManager.SetSidetoneLevel(value, CurrentDeviceInfo!.ID).Wait();
                _deviceManager.SetSidetoneLevelAsync(CurrentDeviceInfo!.ID.ToString(), value).Wait();
                DeviceInfoDTP!.SidetoneLevel = value;
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
                        _log.Info($"[HeadsetViewModel] SetAncModeAsync ....... {value.ToString()}");
                        //_deviceManager.SetAncMode(1, CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetAncModeAsync(CurrentDeviceInfo!.ID.ToString(), 1).Wait();
                        DeviceInfoDTP.AncMode = 1;
                        _isTransparencyChecked = false;
                        _isNoiseOffChecked = false;
                        OnPropertyChanged(nameof(IsTransparencyChecked));
                        OnPropertyChanged(nameof(IsNoiseOffChecked));
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
                        _log.Info($"[HeadsetViewModel] SetAncModeAsync ....... {value.ToString()}");
                        //_deviceManager.SetAncMode(2, CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetAncModeAsync(CurrentDeviceInfo!.ID.ToString(), 2).Wait();
                        DeviceInfoDTP.AncMode = 2;

                        _isActiveNoiseCancellingChecked = false;
                        _isNoiseOffChecked = false;
                        OnPropertyChanged(nameof(IsActiveNoiseCancellingChecked));
                        OnPropertyChanged(nameof(IsNoiseOffChecked));
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

                if (_isNoiseOffChecked != value)
                {
                    _isNoiseOffChecked = value;
                    if (_isNoiseOffChecked)
                    {
                        _log.Info($"[HeadsetViewModel] SetAncModeAsync ....... {value.ToString()}");
                        //_deviceManager.SetAncMode(0, CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetAncModeAsync(CurrentDeviceInfo!.ID.ToString(), 0).Wait();
                        DeviceInfoDTP.AncMode = 0;

                        _isActiveNoiseCancellingChecked = false;
                        _isTransparencyChecked = false;
                        OnPropertyChanged(nameof(IsActiveNoiseCancellingChecked));
                        OnPropertyChanged(nameof(IsTransparencyChecked));
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
                _log.Info($"[HeadsetViewModel] SetAncGainAsync ....... {value.ToString()}");
                //CurrentDeviceInfo!.AncGain = value;
                //_deviceManager.SetAncGain(value, CurrentDeviceInfo!.ID).Wait();
                _deviceManager.SetAncGainAsync(CurrentDeviceInfo!.ID.ToString(), value);
                DeviceInfoDTP.AncGain = value;
                _isTransparencylevelSliderValue = value;
                OnPropertyChanged(nameof(TransparencylevelSliderValue));
            }
        }

        #endregion Group 1

        //Group 2

        #region Group 2

        private bool _isCollaborationChecked = true;
        private bool _isMultimediaChecked;

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
                    //_deviceManager.SetCollaborationMicEnable(value, CurrentDeviceInfo!.ID).Wait();
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
                        _log.Info($"[HeadsetViewModel] SetSelectedPresetAsync ....... {value.ToString()}");
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        //_deviceManager.SetSelectedPreset(1, CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), 1).Wait();
                        DeviceInfoDTP.SelectedPreset = 1;

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
                        _log.Info($"[HeadsetViewModel] SetSelectedPresetAsync ....... {value.ToString()}");
                        _isDefaultChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        //_deviceManager.SetSelectedPreset(2, CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), 2).Wait();
                        DeviceInfoDTP.SelectedPreset = 2;

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
                        _log.Info($"[HeadsetViewModel] SetSelectedPresetAsync ....... {value.ToString()}");
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        //_deviceManager.SetSelectedPreset(3, CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), 3).Wait();
                        DeviceInfoDTP.SelectedPreset = 3;

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
                        _log.Info($"[HeadsetViewModel] SetSelectedPresetAsync ....... {value.ToString()}");
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isCustomChecked = false;
                        //_deviceManager.SetSelectedPreset(4, CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), 4).Wait();
                        DeviceInfoDTP.SelectedPreset = 4;

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
                        _log.Info($"[HeadsetViewModel] SetSelectedPresetAsync ....... {value.ToString()}");
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _audioEqualizerGridPageShow = true;
                        //CurrentDeviceInfo!.SelectedPreset = 101;
                        //_deviceManager.SetSelectedPreset(101, CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetSelectedPresetAsync(CurrentDeviceInfo!.ID.ToString(), 101).Wait();
                        DeviceInfoDTP.SelectedPreset = 101;

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
                {
                    int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 0, 1);
                    _log.Info($"[HeadsetViewModel] SetWearDetectionAsync WearDetectionStatus On ....... {setWear.ToString()}");
                    //_deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 0, 1), CurrentDeviceInfo!.ID).Wait();
                    //_deviceManager.SetWearDetectionForCLI(1, CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                    DeviceInfoDTP.WearDetection = setWear;
                }
                else
                {
                    int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 0, 0);
                    _log.Info($"[HeadsetViewModel] SetWearDetectionAsync WearDetectionStatus Off ....... {setWear.ToString()}");
                    //_deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 0, 0), CurrentDeviceInfo!.ID).Wait();
                    //_deviceManager.SetWearDetectionForCLI(0, CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                    DeviceInfoDTP.WearDetection = setWear;
                }
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
                {
                    int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 1, 1);
                    _log.Info($"[HeadsetViewModel] SetWearDetectionAsync PauseMusicStatus On ....... {setWear.ToString()}");
                    //_deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 1, 1), CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                    DeviceInfoDTP.WearDetection = setWear;
                }
                else
                {
                    int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 1, 0);
                    _log.Info($"[HeadsetViewModel] SetWearDetectionAsync PauseMusicStatus Off ....... {setWear.ToString()}");
                    //_deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 1, 0), CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                    DeviceInfoDTP.WearDetection = setWear;
                }
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
                {
                    int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 2, 1);
                    _log.Info($"[HeadsetViewModel] SetWearDetectionAsync MuteMicrophoneStatus On ....... {setWear.ToString()}");
                    //_deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 2, 1), CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                    DeviceInfoDTP.WearDetection = setWear;
                }
                else
                {
                    int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 2, 0);
                    _log.Info($"[HeadsetViewModel] SetWearDetectionAsync MuteMicrophoneStatus Off ....... {setWear.ToString()}");
                    //_deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 2, 0), CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                    DeviceInfoDTP.WearDetection = setWear;
                }
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
                {
                    int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 4, 1);
                    _log.Info($"[HeadsetViewModel] SetWearDetectionAsync QuickPauseStatus On ....... {setWear.ToString()}");
                    //_deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 4, 1), CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                    DeviceInfoDTP.WearDetection = setWear;
                }
                else
                {
                    int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 4, 0);
                    _log.Info($"[HeadsetViewModel] SetWearDetectionAsync QuickPauseStatus Off ....... {setWear.ToString()}");
                    //_deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 4, 0), CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                    DeviceInfoDTP.WearDetection = setWear;
                }
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
                if (_isNormal2Checked != value)
                {
                    _isNormal2Checked = value;
                    if (_isNormal2Checked)
                    {
                        IsLowChecked = false;
                    }
                    if (value)
                    {
                        int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 3, 1);
                        _log.Info($"[HeadsetViewModel] SetWearDetectionAsync IsNormal2Checked On ....... {setWear.ToString()}");
                        //_deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 3, 1), CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                        DeviceInfoDTP.WearDetection = setWear;
                    }
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
                    {
                        int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 3, 0);
                        _log.Info($"[HeadsetViewModel] SetWearDetectionAsync IsLowChecked On ....... {setWear.ToString()}");
                        //_deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 3, 0), CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                        DeviceInfoDTP.WearDetection = setWear;
                    }
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
                    {
                        int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 5, 1);
                        _log.Info($"[HeadsetViewModel] SetWearDetectionAsync IsNormalChecked On ....... {setWear.ToString()}");
                        //_deviceManager.SetWearDetection((int)SetBitsValue((uint)CurrentDeviceInfo!.WearDetection, 5, 1), CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString(), setWear).Wait();
                        DeviceInfoDTP.WearDetection = setWear;
                    }
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
                    {
                        int setWear = (int)SetBitsValue((uint)DeviceInfoDTP!.WearDetection, 5, 2);
                        _log.Info($"[HeadsetViewModel] SetWearDetectionAsync IsSensitiveChecked On ....... {setWear.ToString()}");
                        //_deviceManager.SetWearDetection((int)SetBitsValue((uint)CurrentDeviceInfo!.WearDetection, 5, 2), CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetWearDetectionAsync(CurrentDeviceInfo!.ID.ToString() , setWear).Wait();
                        DeviceInfoDTP.WearDetection = setWear;
                    }
                    OnPropertyChanged(nameof(IsSensitiveChecked));
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
        private string _isBusyLight_String = "ON";

        public string BusyLight_String
        {
            get => _isBusyLightStatus ? "ON" : "OFF";
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
                _isBusyLightStatus = DeviceInfoDTP.BusyLight;//_deviceManager.GetBusyLightAsync(CurrentDeviceInfo!.ID.ToString()).Result; //CurrentDeviceInfo!.BusyLight;
                return _isBusyLightStatus;
            }
            set
            {
                _log.Info($"[HeadsetViewModel] SetBusyLightAsync ....... {value.ToString()}");
                //_deviceManager.SetBusyLight(value, CurrentDeviceInfo!.ID).Wait();
                _deviceManager.SetBusyLightAsync(CurrentDeviceInfo!.ID.ToString(), value);
                _isBusyLightStatus = value;
                DeviceInfoDTP.BusyLight = value;
                OnPropertyChanged("BusyLight_String");
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
                        _log.Info($"[HeadsetViewModel] SetVoiceGuidanceAsync ....... {false.ToString()}");
                        _isAllChecked = false;
                        //_deviceManager.SetVoiceGuidance(false, CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetVoiceGuidanceAsync(CurrentDeviceInfo!.ID.ToString(), false).Wait();
                        DeviceInfoDTP.VoiceGuidance = false;

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
                        _log.Info($"[HeadsetViewModel] SetVoiceGuidanceAsync ....... {true.ToString()}");
                        _isEssentialChecked = false;
                        //_deviceManager.SetVoiceGuidance(true, CurrentDeviceInfo!.ID).Wait();
                        _deviceManager.SetVoiceGuidanceAsync(CurrentDeviceInfo!.ID.ToString(), true).Wait();
                        DeviceInfoDTP.VoiceGuidance = true;

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
            set { 
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
            set {
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
    }
}