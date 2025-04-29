#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// IDTPProxyPlugin.cs created on 8/13/2024T3:37 PM
//

#endregion

using Dell.Client.Framework.Common;
using Dell.TechHub.Commodity.Peripheral;
using DPeMPublic.Common.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface IDTPProxyPlugin : IFrameworkPlugin
    {
        event EventHandler<UpdateDTPProxyNotify> DTPProxyPluginSDKeventHandler;
        Task<bool> GetDTPProxyPluginReady();

        #region globalperipheral
        Task<bool> GetIsLockKeyNotificationsEnabledValue();

        Task<bool> GetIsBatteryNotificationsEnabledValue();
        Task<bool> GetIsPresenceDetectionSensnorStateNotificationsEnabledValue();
        Task<bool> GetIsAnalyticsEnabledValue();
        Task<bool> GetIsQuickAccessMenuEnabledValue();

        Task<bool> GetIsMuteStatusNotificationsEnabledValue();
        Task<bool> GetIsQuickAccessMenuOSDEnabledValue();
        Task<bool> SetIsLockKeyNotificationsEnabledValue(bool newValue);
        Task<bool> SetIsBatteryNotificationsEnabledValue(bool newValue);
        Task<bool> SetIsPresenceDetectionSensnorStateNotificationsEnabledValue(bool newValue);
        Task<bool> SetIsAnalyticsEnabledValue(bool newValue);
        Task<bool> SetIsQuickAccessMenuEnabledValue(bool newValue);
        Task<bool> SetIsMuteStatusNotificationsEnabledValue(bool newValue);
        Task<bool> SetIsQuickAccessMenuOSDEnabledValue(bool newValue);
        #endregion globalperipheral

        #region Mouse

        Task<int> GetDpiValue(string Guid);
        Task<JArray> GetMouseProgrammableKeys(string Guid);
        Task<JArray> GetAppSpecificProfiles(string Guid);
        Task<bool> DeleteMouseAllAssignedActions(string Guid);
        Task<JArray> GetMouseAssignableActions(string Guid);
        Task<JArray> GetMouseAssignedActions(string Guid);
        Task<string> GetMouseKeystrokeDisplayData(string Guid);
        Task<bool> StartMouseKeystrokeRecording(string Guid);
        Task<bool> StopMouseKeystrokeRecording(string Guid);
        Task<int> GetTouchScrollSensitivityLevel(string Guid);

        Task SetDpiValue(string Guid, int newValue);
        Task SetMouseAction(string Guid, byte[] newValue);
        Task SetCurrentSelectedAppSpecificProfile(string Guid, string newValue);
        Task DeleteMouseAssignedAction(string Guid, int newValue);
        Task SetMouseAssignDialogAction(string Guid, byte[] newValue);
        Task SetMouseAssignKeystrokeAction(string Guid, byte[] newValue);
        Task<bool> RestoreToDefaultMouse(string Guid, bool isFromCli = true);
        Task<bool> SetReportRate(string Guid, int newValue);
        Task<bool> SetTouchScrollSensitivityLevel(string Guid, int newValue);

        #endregion

        #region Keyboard

        Task<JArray> GetKeyboardDeviceItemsEx();
        Task<JArray> GetKbProgrammableKeys(string Guid);
        Task<bool> DeleteKeyboardAllAssignedActions(string Guid);
        Task<JArray> GetKbAssignableActions(string Guid);
        Task<JArray> GetKbAssignedActions(string Guid);
        Task<string> GetKeyboardKeystrokeDisplayData(string Guid);
        Task<bool> StartKeyboardKeystrokeRecording(string Guid);
        Task<bool> StopKeyboardKeystrokeRecording(string Guid);


        Task DeleteKeyboardAssignedAction(string Guid, int newValue);
        Task SetKbAssignedAction(string Guid, byte[] newValue);
        Task SetKbAssignDialogAction(string Guid, byte[] newValue);
        Task SetKbAssignKeystrokeAction(string Guid, byte[] newValue);
        Task<bool> RestoreToDefaultKB(string Guid);

        #endregion

        #region Pen
        Task SetEraserDoublePressSetting(string itemID, byte[] newValue);

        Task SetEraserLongPressSetting(string itemID, byte[] newValue);

        Task SetEraserSinglePressSetting(string itemID, byte[] newValue);

        Task SetIsSideBottomButtonHoverClick(string itemID, bool newValue);

        Task SetIsSideTopButtonHoverClick(string itemID, bool newValue);

        Task SetMenuSinglePressSetting(string itemID, byte[] newValue);

        Task SetMenuCenterRightClickSetting(string itemID, bool newValue);

        Task SetSideBottomSwitchSinglePressSetting(string itemID, byte[] newValue);

        Task SetSideTopSwitchSinglePressSetting(string itemID, byte[] newValue);

        Task SetTiltSensitivity(string itemID, int newValue);

        Task SetTipSensitivity(string itemID, int newValue);
        Task<bool> RestoreToDefaultPen();
        Task<bool> RestoreRadialMenuToDefault();


        Task<string> PairingPen();
        Task UnPairPen(string Guid);
        Task<JArray> GetPenDeviceItemsEx();
        Task<string> GetEraserDoublePressValues();
        Task<string> GetEraserSinglePressValues();
        Task<string> GetEraserLongPressValues();
        Task<string> GetSideSwitchSinglePressValues();
        Task<string> GetMenuSinglePressValues();
        Task<string> GetLaunchableAppValues();
        Task<string> GetEraserDoublePressSetting();
        Task<string> GetEraserSinglePressSetting();
        Task<string> GetEraserLongPressSetting();
        Task<string> GetSideTopSwitchSinglePressSetting();
        Task<string> GetSideBottomSwitchSinglePressSetting();
        Task<string> GetMenuSinglePressSetting();
        Task<bool> GetMenuCenterRightClickSetting();
        Task<bool> GetIsSideTopButtonHoverClick();
        Task<bool> GetIsSideBottomButtonHoverClick();
        Task<bool> StartKeyCapturePen();
        Task<bool> FinishKeyCapturePen();
        Task<string> KeyCaptureData();
        Task<string> GetIsdDriverVersion();

        #endregion

        #region webcam

        /// <summary>
        /// Webcam change event
        /// </summary>
        //event EventHandler<bool>? Esi_IsCameraSensorCover_ChangeEvent;
        //event EventHandler<int>? WALSnoozeTimeLeftInSeconds_ChangeEvent;
        //event EventHandler<bool>? Esi_IsWALLockCountdownStartedChanged_ChangeEvent;
        //event EventHandler<int>? Esi_WALLockCountdownChanged_ChangeEvent;
        event EventHandler<UpdateUINotify>? DTPEventHandler;

        event EventHandler<CMAIDEventArgs>? CMAEventHandler;

        Task<JArray> GetPresetProfiles(string Guid);
        Task<JArray> GetCustomProfiles(string Guid);
        Task<string> GetProfile(string Guid);
        Task<string> GetProfileName(string Guid);
        Task<int> GetBrightness(string Guid);
        Task<string> GetCameraFirmwareVersion(string Guid);
        Task<bool> GetIsWindowsHelloCapabilityVerified(string Guid);
        Task<bool> GetIsAllSupportedResolutionsFound(string Guid);
        Task<bool> GetIsPropertyFOVSupported(string Guid);
        Task<int> GetFieldOfView(string Guid);
        Task<bool> GetIsPropertyHDRSupported(string Guid);
        Task<bool> GetIsHDROn(string Guid);
        Task<bool> GeIsPropertyAntiFlickerSupported(string Guid);
        Task<int> GetAntiFlicker(string Guid);
        Task<bool> GetIsPropertyAutoFramingSupported(string Guid);
        Task<bool> GetIsAutoFramingOn(string Guid);
        Task<string> GetSupportedResolutions(string Guid);
        Task<string> GetSelectedResolution(string Guid);
        Task<int> GetZoom(string Guid);
        Task<int> GetFocus(string Guid);
        Task<bool?> GetIsFocusOn(string Guid);
        Task<int> GetPriority(string Guid);
        Task<bool?> GetIsAutoFramingTransitionOn(string Guid);
        Task<int> GetAutoFramingFrameSize(string Guid);
        Task<int> GetAutoFramingSensitivity(string Guid);
        Task<string> GetWebcamSerialNumber(string Guid);
        Task<int> GetBgBlur(string Guid);
        Task<bool> GetIsBgBlurEnable(string Guid);
        Task<bool> GetIsPropertyBgBlurSupported(string Guid);
        Task SetIsMicEnumerationOn(string Guid, bool newValue);
        Task SetProfile(string Guid, string newValue);
        Task SetProfileName(string Guid, string newValue);
        Task CreateCustomProfile(string Guid, string newValue);
        Task DeleteProfile(string Guid, string newValue);
        Task<bool> SetZoom(string Guid, int newValue);
        Task<bool> SetIsAutoFramingOn(string Guid, bool newValue);
        Task<bool> SetIsAutoFramingTransitionOn(string Guid, bool newValue);
        Task<bool> SetAutoFramingSensitivity(string Guid, int newValue);
        Task<bool> SetAutoFramingFrameSize(string Guid, int newValue);
        Task<bool> SetFieldOfView(string Guid, int newValue);
        Task<bool> SetIsFocusOn(string Guid, bool newValue);
        Task<bool> SetFocus(string Guid, int newValue);
        Task<bool> SetPriority(string Guid, int newValue);
        Task<bool> SetIsHDROn(string Guid, bool newValue);
        Task<bool> SetIsAutoWhiteBalanceOn(string Guid, bool newValue);
        Task<bool> SetAutoWhiteBalance(string Guid, int newValue);
        Task<bool> SetBrightness(string Guid, int newValue);
        Task<bool> SetSharpness(string Guid, int newValue);
        Task<bool> SetContrast(string Guid, int newValue);
        Task<bool> SetSaturation(string Guid, int newValue);
        Task<bool> SetIsBgBlurEnable(string Guid, bool newValue);
        Task<bool> SetBgBlur(string Guid, int newValue);
        Task SetAntiFlicker(string Guid, int newValue);
        Task SetTilt(string Guid, int newValue);
        Task SetPan(string Guid, int newValue);

        // webcam presence detection
        Task SetWALTime(string Guid, int newValue);
        Task SetSnooze(string Guid, int newValue);
        Task SetSnoozeLength(string Guid, int newValue);
        Task SetIsProximitySensorEnable(string Guid, bool newValue);
        Task SetIsWakeonApproachEnable(string Guid, bool newValue);
        Task SetIsWalkAwayLockEnable(string Guid, bool newValue);
        Task SetIsPrioritizeExternalWebcam(string Guid, bool newValue);
        Task ResetToDefault_webcam(string Guid, bool newValue);

        Task<int> GetWALTime(string Guid);
        Task<int> GetSnooze(string Guid);
        Task<int> GetSnoozeLength(string Guid);
        Task<bool> GetIsProximitySensorEnable(string Guid);
        Task<bool> GetIsWakeonApproachEnable(string Guid);
        Task<bool> GetIsWalkAwayLockEnable(string Guid);
        Task<bool?> GetIsPrioritizeExternalWebcam(string Guid);
        Task<bool> GetIsZoomMeetingActive(string Guid);
        //Task<bool> GetZoomMeetingType(string Guid);
        Task<bool> GetIsZoomMeetingActive();
        Task<int> GetZoomMeetingTypeAsync(string Guid);
        Task<bool> GetIsZoomScreenShareActive(string Guid);

        //Marked by Derek 1121
        //event EventHandler<ZoomChangedArgs> ZoomChanged_Notify;
        //event EventHandler<ZoomMeetingTypeChangedArgs> ZoomMeetingTypeChanged_Notify;
        //event EventHandler<IsZoomMeetingActiveChangedArgs> IsZoomMeetingActive_Notify;
        //event EventHandler<IsZoomScreenShareActiveChangedArgs> IsZoomScreenShareActive_Notify;

        Task<bool> GetIsESISupported(string Guid);

        //Derek 1221 for QAM
        Task<string> GetWebcamDeviceID();
        Task<int> GetWebcamDeviceCountAsync();  //Derek 2025/02/19

        #endregion

        #region Headset

        Task<bool> SetMicNoiseCancellationAsync(string Guid, bool newValue);
        Task<bool> SetSidetoneAsync(string Guid, bool newValue);
        Task<bool> SetBusyLightAsync(string Guid, bool newValue);
        Task<bool> SetVoiceGuidanceAsync(string Guid, bool newValue);
        Task<bool> SetSelectedPresetAsync(string Guid, int newValue);
        Task<bool> SetSidetoneLevelAsync(string Guid, int newValue);
        Task<bool> SetBandsGainAsync(string Guid, byte[] newValue);
        Task<bool> SetBand1GainAsync(string Guid, int newValue);
        Task<bool> SetBand2GainAsync(string Guid, int newValue);
        Task<bool> SetBand3GainAsync(string Guid, int newValue);
        Task<bool> SetBand4GainAsync(string Guid, int newValue);
        Task<bool> SetBand5GainAsync(string Guid, int newValue);
        Task<bool> SetAncModeAsync(string Guid, int newValue);
        Task<bool> SetAncGainAsync(string Guid, int newValue);
        Task<bool> SetWearDetectionAsync(string Guid, bool newValue);

        Task<bool> SetIsWearDetectionMuteMicEnabledAsync(string Guid, bool newValue);

        Task<bool> SetIsWearDetectionPauseMusicEnabledAsync(string Guid, bool newValue);

        Task<bool> SetWearDetectionQuickPauseAsync(string Guid, int newValue);

        Task<bool> SetWearDetectionSensitivityAsync(string Guid, int newValue);

        Task<bool> SetMicNCIncomingAsync(string Guid, bool newValue);
        Task<bool> SetUnPairAsync(string Guid, bool newValue);
        Task<bool> SetFactoryResetAsyncValueForHeadset(string Guid, bool newValue);

        Task<bool> SetFactoryResetAsyncValueForHeadsetForCLI(string Guid, bool newValue);
        Task<bool> SetBoomMicAsync(string Guid, bool newValue);

        //Peripheral Common Properties Get
        Task<JArray> GetHeadsetDeviceItemsExAsync();
        Task<DeviceInterfaceType> GetHeadsetInterfaceTypeAsync(string Guid);
        Task<string> GetHeadsetDeviceNameAsync(string Guid);
        Task<string> GetHeadsetDeviceIdAsync(string Guid);
        Task<string> GetHeadsetPluginIdAsync(string Guid);
        Task<int> GetHeadsetODMIdAsync(string Guid);
        Task<string> GetHeadsetModelNumberAsync(string Guid);
        Task<int> GetHeadsetInstanceNumberAsync(string Guid);
        Task<int> GetHeadsetInstanceIdAsync(string Guid);
        Task<string> GetHeadsetFirmwareVersionAsync(string Guid);
        Task<string> GetHeadsetDeviceTypeAsync(string Guid);

        //////////Headset Get//////////
        Task<string> GetHeadsetParentDeviceTypeAsync(string Guid);
        Task<bool> GetHeadsetIsBatteryLevelSupportedAsync(string Guid);
        Task<int> GetHeadsetBatteryLevelAsync(string Guid);
        Task<string> GetHeadsetDeviceBatteryStatusAsync(string Guid);
        Task<string> GetHeadsetPairingStatusAsync(string Guid);

        Task<string> GetHeadsetPairedHostName1Async(string Guid);

        Task<string> GetHeadsetPairedHostName2Async(string Guid);

        Task<string> GetHeadsetPairedHostName3Async(string Guid);

        Task<int> GetHeadsetMaxPairingSlotsAsync(string Guid);
        Task<int> GetHeadsetPairedDeviceCountAsync(string Guid);
        Task<int> GetHeadsetTotalNumberOfPairedHostNameAsync(string Guid);
        Task<string> GetHeadsetSerialNumberAsync(string Guid);
        Task<bool> GetIsReadyAsync(string Guid);
        Task<bool> GetIsDirtyAsync(string Guid);
        Task<bool> GetIsMicNoiseCancellationSupportedAsync(string Guid);
        Task<bool> GetIsSidetoneSupportedAsync(string Guid);
        Task<bool> GetIsBusyLightSupportedAsync(string Guid);
        Task<bool> GetIsVoiceGuidanceSupportedAsync(string Guid);
        Task<bool> GetIsPresetsSupportedAsync(string Guid);
        Task<bool> GetIsEqualizerSupportedAsync(string Guid);
        Task<HeadsetConnectionType> GetConnectionTypeAsync(string Guid);
        Task<bool> GetIsANCSupportedAsync(string Guid);
        Task<bool> GetIsWearDetectionSupportedAsync(string Guid);
        Task<bool> GetIsWearDetectionSensitivitySupportedAsync(string Guid);
        Task<bool> GetIsWearDetectionPauseMusicSupportedAsync(string Guid);
        Task<bool> GetIsWearDetectionMuteMicSupportedAsync(string Guid);
        Task<bool> GetIsWearDetectionQuickPauseSupportedAsync(string Guid);
        Task<bool> GetMicNoiseCancellationAsync(string Guid);
        Task<bool> GetMicNCIncomingAsync(string Guid);
        Task<bool> GetSidetoneAsync(string Guid);
        Task<bool> GetBusyLightAsync(string Guid);
        Task<bool> GetVoiceGuidanceAsync(string Guid);
        Task<int> GetSelectedPresetAsync(string Guid);
        Task<int> GetSidetoneLevelAsync(string Guid);
        Task<bool> GetMuteStatusAsync(string Guid);
        Task<byte[]> GetBandsGainAsync(string Guid);
        Task<int> GetBand1GainAsync(string Guid);
        Task<int> GetBand2GainAsync(string Guid);
        Task<int> GetBand3GainAsync(string Guid);
        Task<int> GetBand4GainAsync(string Guid);
        Task<int> GetBand5GainAsync(string Guid);
        Task<int> GetAncModeAsync(string Guid);
        Task<int> GetAncGainAsync(string Guid);
        Task<bool> GetWearDetectionAsync(string Guid);
        Task<bool> GetIsWearDetectionPauseMusicEnabledAsync(string Guid);

        Task<bool> GetIsWearDetectionMuteMicEnabledAsync(string Guid);

        Task<int> GetWearDetectionSensitivityAsync(string Guid);

        Task<int> GetWearDetectionQuickPauseAsync(string Guid);

        Task<bool> GetIsMicNCIncomingSupportedAsync(string Guid);

        Task<bool> GetIsBoomMicSupportedAsync(string Guid);

        Task<bool> GetBoomMicAsync(string Guid);

        public void SendHeadsetEventToUI(string sendMsg);
        #endregion

        #region Wired Audio

        Task<bool> SetBassAsync(string Guid, int newValue);
        Task<bool> SetMidRangeAsync(string Guid, int newValue);
        Task<bool> SetTrebleAsync(string Guid, int newValue);
        Task<bool> SetProfileForSpeaker(string Guid, string newValue);
        Task<bool> SetIsWiredAudioMicMuteSoundEnableAsync(string Guid, bool newValue);
        Task<bool> SetWiredAudioVolumeAdjustmentToneAsync(string Guid, int newValue);
        Task<bool> SetIsWiredAudioIMicNSEnableAsync(string Guid, bool newValue);
        Task<bool> SetResetToDefaultAsyncForSoundbar(string Guid, bool newValue);

        ////////////////////////////////Get////////////////////////////////

        Task<string> GetWiredAudioSerialNumberAsync(string item);
        Task<string> GetProfileNameAsync(string item);
        Task<string> GetProfileAsync(string item);
        Task<int> GetBassAsync(string Guid);
        Task<int> GetMidRangeAsync(string Guid);
        Task<int> GetTrebleAsync(string Guid);
        Task<bool> GetIsWiredAudioMicMuteSoundEnableAsync(string Guid);
        Task<int> GetWiredAudioVolumeAdjustmentToneAsync(string Guid);
        Task<bool> GetIsWiredAudioIMicNSEnableAsync(string Guid);
        Task<bool> GetIsAudioEqualizerSupportedAsync(string Guid);
        Task<bool> GetMuteStatusAsyncForSpeaker(string Guid);
        Task<bool> GetIsIMicNSSupportedAsync(string Guid);
        Task<bool> GetIsVolumeAdjustmentToneSupportedAsync(string Guid);
        Task<bool> GetIsMicMuteSoundSupportedAsync(string Guid);
        Task<bool> GetPresetProfilesAsync(string Guid);
        Task<bool> GetIsBassEqualizerSupportedAsync(string Guid);
        Task<bool> GetIsMidRangeEqualizerSupportedAsync(string Guid);
        Task<bool> GetIsTrebleEqualizerSupportedAsync(string Guid);

        #endregion

        #region Dongle
        Task<string> GetFirmwareVersionAsyncForDongle(string Guid);
        Task<string> GetConnectedDeviceInfoAsyncForDongle(string Guid);
        Task<string> GetDeviceIdAsyncForDongle(string Guid);
        Task<string> GetPluginIdAsyncForDongle(string Guid);
        Task<JArray> GetDeviceItemsExAsyncForDongle(string Guid);

        #endregion

        #region Dock
        Task<DockData> GetDockData(string guid);
        Task<string> GetFirmwareVersionForDock(string guid);
        Task<string> GetDockServiceTagForDock(string guid);
        #endregion

        #region  IAirAudioCommodity

        #region Get

        Task<HeadsetConnectionType> GetAirAudioConnectionTypeAsync(string Guid);

        Task<JArray> GetAirAudioDeviceItemsAsync();

        Task<string> GetAirAudioSerialNumberAsync(string Guid);

        Task<string> GetAirAudioDeviceBatteryStatusAsync(string Guid);

        Task<string> GetAirAudioPairingHostName1Async(string Guid);

        Task<string> GetAirAudioPairingHostName2Async(string Guid);

        Task<string> GetAirAudioPairingHostName3Async(string Guid);

        Task<string> GetAirAudioPairingStatusNameAsync(string Guid);

        Task<string> GetAirAudioParentDeviceTypeAsync(string Guid);

        Task<string> GetAirAudioModelNumberAsync(string Guid);

        Task<string> GetAirAudioDeviceTypeAsync(string Guid);

        Task<string> GetAirAudioFirmwareVersionAsync(string Guid);

        Task<string> GetAirAudioPluginIdAsync(string Guid);

        Task<string> GetAirAudioDeviceIdAsync(string Guid);

        Task<string> GetAirAudioDeviceNameAsync(string Guid);
        Task<string> GetAirAudioSerialNumberCaseAsync(string Guid);

        Task<string> GetAirAudioBatteryStatusLeftAsync(string Guid);
        Task<string> GetAirAudioBatteryStatusRightAsync(string Guid);
        Task<string> GetAirAudioBatteryStatusCaseAsync(string Guid);
        Task<DeviceInterfaceType> GetAirAudioDeviceInterfaceTypeAsync(string Guid);

        //Task<bool> GetAirAudioIsWearDetectionAsync(string Guid);
        Task<bool> GetAirAudioIsAutoPowerOffEnabledAsync(string Guid);
        Task<bool> GetAirAudioMuteStatusAsync(string Guid);

        Task<bool> GetAirAudioBoomMicAsync(string Guid);

        Task<bool> GetAirAudioIsBoomMicSupportedAsync(string Guid);

        Task<bool> GetAirAudioWearDetectionAsync(string Guid);

        Task<bool> GetAirAudioVoiceGuidanceAsync(string Guid);

        Task<bool> GetAirAudioBusyLightAsync(string Guid);

        Task<bool> GetAirAudioSidetoneAsync(string Guid);

        Task<bool> GetAirAudioMicNCIncomingAsync(string Guid);

        Task<bool> GetAirAudioIsMicNCIncomingSupportedAsync(string Guid);

        Task<bool> GetAirAudioMicNoiseCancellationAsync(string Guid);

        Task<int> GetAirAudioWearDetectionQuickPauseAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionMuteMicSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionPauseMusicSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionSensitivitySupportedAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsANCSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsEqualizerSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsPresetsSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsVoiceGuidanceSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsBusyLightSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsSidetoneSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsMicNoiseCancellationSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsDirtyAsync(string Guid);

        Task<bool> GetAirAudioIsReadyAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionPauseMusicEnabledAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionMuteMicEnabledAsync(string Guid);
        Task<bool> GetAirAudioIsWearDetectionAnswerCallsEnabledAsync(string Guid);
        Task<bool> GetAirAudioIsBatteryLevelSupportedAsync(string Guid);

        Task<int> GetAirAudioWearDetectionSensitivityAsync(string Guid);
        Task<int> GetAirAudioAutoPowerOffIntervalAsync(string Guid);


        Task<int> GetAirAudioAncGainAsync(string Guid);

        Task<int> GetAirAudioAncModeAsync(string Guid);

        Task<int> GetAirAudioBand1GainAsync(string Guid);

        Task<int> GetAirAudioBand2GainAsync(string Guid);

        Task<int> GetAirAudioBand3GainAsync(string Guid);

        Task<int> GetAirAudioBand4GainAsync(string Guid);

        Task<int> GetAirAudioBand5GainAsync(string Guid);

        Task<int> GetAirAudioSidetoneLevelAsync(string Guid);

        Task<int> GetAirAudioSelectedPresetAsync(string Guid);

        Task<int> GetAirAudioBatteryLevelAsync(string Guid);

        Task<int> GetAirAudioPairedDeviceCountAsync(string Guid);

        Task<int> GetAirAudioMaxPairingSlotsAsync(string Guid);

        Task<int> GetAirAudioTotalNumberOfPairedHostNameAsync(string Guid);

        Task<int> GetAirAudioInstanceIdAsync(string Guid);

        Task<int> GetAirAudioInstanceNumberAsync(string Guid);

        Task<int> GetAirAudioODMIdAsync(string Guid);

        Task<int> GetAirAudioBatteryLevelLeftAsync(string Guid);
        Task<int> GetAirAudioBatteryLevelRightAsync(string Guid);
        Task<int> GetAirAudioBatteryLevelCaseAsync(string Guid);
        Task<int> GetAirAudioMaxAllowedPariedHost(string Guid);

        Task<bool> GetAirAudioIsConnectedAsync(string Guid);

        Task<bool> GetAirAudioIsConnectedLeftAsync(string Guid);

        Task<bool> GetAirAudioIsConnectedRightAsync(string Guid);
        //Task<string> GetAirAudioPairedHostName2Async(string Guid);
        #endregion Get

        #region Set

        Task<bool> SetAirAudioMicNoiseCancellationAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioSidetoneAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioBusyLightAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioVoiceGuidanceAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioSelectedPresetAsync(string Guid, int newValue);

        Task<bool> SetAirAudioSidetoneLevelAsync(string Guid, int newValue);

        Task<bool> SetAirAudioBandsGainAsync(string Guid, byte[] newValue);

        Task<bool> SetAirAudioBand1GainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioBand2GainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioBand3GainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioBand4GainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioBand5GainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioAncModeAsync(string Guid, int newValue);

        Task<bool> SetAirAudioAncGainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioWearDetectionAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioFactoryResetAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioIsBoomMicSupportedAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioWearDetectionQuickPauseAsync(string Guid, int newValue);

        Task<bool> SetAirAudioWearDetectionSensitivityAsync(string Guid, int newValue);

        Task<bool> SetAirAudioMicNCIncomingAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioUnPairAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioIsWearDetectionPauseMusicEnabledAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioIsWearDetectionMuteMicEnabledAsync(string Guid, bool newValue);

        Task<bool> SetFactoryResetAsyncValueForAirAudioAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioIsAutoPowerOffEnabledAsync(string Guid, bool newValue);
        Task<bool> SetAirAudioIsWearDetectionAnswerCallsEnabledAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioAutoPowerOffIntervalAsync(string Guid, int newValue);
        #endregion Set
        #endregion
    }
}