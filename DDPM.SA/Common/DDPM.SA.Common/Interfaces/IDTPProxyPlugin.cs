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
using DPeMPublic.Common.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface IDTPProxyPlugin : IFrameworkPlugin
    {

        #region Mouse

        Task<int> GetDpiValue(string Guid);
        Task<JArray> GetMouseProgrammableKeys(string Guid);
        Task<JArray> GetAppSpecificProfiles(string Guid);
        Task<bool> DeleteMouseAllAssignedActions(string Guid);
        Task<JArray> GetMouseAssignableActions(string Guid);
        Task<string> GetMouseKeystrokeDisplayData(string Guid);
        Task<bool> StartMouseKeystrokeRecording(string Guid);
        Task<bool> StopMouseKeystrokeRecording(string Guid);

        Task SetDpiValue(string Guid, int newValue);
        Task SetMouseAction(string Guid, byte[] newValue);
        Task SetCurrentSelectedAppSpecificProfile(string Guid, string newValue);
        Task DeleteMouseAssignedAction(string Guid, int newValue);
        Task SetMouseAssignDialogAction(string Guid, byte[] newValue);
        Task SetMouseAssignKeystrokeAction(string Guid, byte[] newValue);


        #endregion

        #region Keyboard

        Task<JArray> GetKeyboardDeviceItemsEx();
        Task<JArray> GetKbProgrammableKeys(string Guid);
        Task<bool> DeleteKeyboardAllAssignedActions(string Guid);
        Task<JArray> GetKbAssignableActions(string Guid);


        Task DeleteKeyboardAssignedAction(string Guid, int newValue);
        Task SetKbAssignedAction(string Guid, string newValue);
        Task SetKbAssignDialogAction(string Guid, string newValue);
        Task SetKbAssignKeystrokeAction(string Guid, string newValue);

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
        Task ResetToDefault_Pen();


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

        #endregion

        #region webcam

        /// <summary>
        /// Webcam change event
        /// </summary>
        event EventHandler<bool>? IsCameraSensorCovere_ChangeEvent;
        event EventHandler<int>? WALSnoozeTimeLeftInSeconds_ChangeEvent;
        event EventHandler<bool>? Esi_IsWALLockCountdownStartedChanged_ChangeEvent;
        event EventHandler<int>? Esi_WALLockCountdownChanged_ChangeEvent;

        Task<JArray> GetPresetProfiles(string Guid);
        Task<JArray> GetCustomProfiles(string Guid);
        Task<string> GetProfile(string Guid);
        Task<string> GetProfileName(string Guid);
        Task<int> GetBrightness(string Guid);
        Task<string> GetCameraFirmwareVersion(string Guid);
        Task<bool> GetIsPropertyFOVSupported(string Guid);
        Task<int> GetFieldOfView(string Guid);
        Task<bool> GetIsPropertyHDRSupported(string Guid);
        Task<bool> GetIsHDROn(string Guid);
        Task<bool> CheckIsPropertyAntiFlickerSupported(string Guid);
        Task<int> GetAntiFlicker(string Guid);
        Task<bool> GetIsPropertyAutoFramingSupported(string Guid);
        Task<bool> GetIsAutoFramingOn(string Guid);
        Task<string> GetSupportedResolutions(string Guid);
        Task<string> GetSelectedResolution(string Guid);

        Task SetIsMicEnumerationOn(string Guid, bool newValue);
        Task SetProfile(string Guid, string newValue);
        Task SetProfileName(string Guid, string newValue);
        Task CreateCustomProfile(string Guid, string newValue);
        Task DeleteProfile(string Guid, string newValue);
        Task SetZoom(string Guid, int newValue);
        Task SetIsAutoFramingOn(string Guid, bool newValue);
        Task SetIsAutoFramingTransitionOn(string Guid, bool newValue);
        Task SetAutoFramingSensitivity(string Guid, int newValue);
        Task SetAutoFramingFrameSize(string Guid, int newValue);
        Task SetFieldOfView(string Guid, int newValue);
        Task SetIsFocusOn(string Guid, bool newValue);
        Task SetFocus(string Guid, int newValue);
        Task SetPriority(string Guid, int newValue);
        Task SetIsHDROn(string Guid, bool newValue);
        Task SetIsAutoWhiteBalanceOn(string Guid, bool newValue);
        Task SetAutoWhiteBalance(string Guid, int newValue);
        Task SetBrightness(string Guid, int newValue);
        Task SetSharpness(string Guid, int newValue);
        Task SetContrast(string Guid, int newValue);
        Task SetSaturation(string Guid, int newValue);
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
        Task<bool> GetIsPrioritizeExternalWebcam(string Guid);

        Task<bool> GetIsESISupported(string Guid);

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
        Task<bool> SetWearDetectionAsync(string Guid, int newValue);
        Task<bool> SetMicNCIncomingAsync(string Guid, bool newValue);
        Task<bool> SetUnPairAsync(string Guid, bool newValue);
        Task<bool> SetFactoryResetAsyncValueForHeadset(string Guid, bool newValue);

        //Peripheral Common Properties Get
        Task<JArray> GetDeviceItemsExAsync(string Guid);
        Task<DeviceInterfaceType> GetInterfaceTypeAsync(string Guid);
        Task<string> GetDeviceNameAsync(string Guid);
        Task<string> GetDeviceIdAsync(string Guid);
        Task<string> GetPluginIdAsync(string Guid);
        Task<int> GetODMIdAsync(string Guid);
        Task<string> GetModelNumberAsync(string Guid);
        Task<int> GetInstanceNumberAsync(string Guid);
        Task<int> GetInstanceIdAsync(string Guid);
        Task<string> GetFirmwareVersionAsync(string Guid);
        Task<string> GetDeviceTypeAsync(string Guid);

        //////////Headset Get//////////
        Task<string> GetParentDeviceTypeAsync(string Guid);
        Task<bool> GetIsBatteryLevelSupportedAsync(string Guid);
        Task<int> GetBatteryLevelAsync(string Guid);
        Task<string> GetDeviceBatteryStatusAsync(string Guid);
        Task<string> GetPairingStatusAsync(string Guid);
        Task<int> GetMaxPairingSlotsAsync(string Guid);
        Task<int> GetPairedDeviceCountAsync(string Guid);
        Task<int> GetTotalNumberOfPairedHostNameAsync(string Guid);
        Task<string> GetSerialNumberAsync(string Guid);
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
        Task<int> GetWearDetectionAsync(string Guid);
        Task<bool> GetIsMicNCIncomingSupportedAsync(string Guid);

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

        Task<string> GetProfileAsync(string item);
        Task<int> GetBassAsync(string Guid);
        Task<int> GetMidRangeAsync(string Guid);
        Task<int> GetTrebleAsync(string Guid);
        Task<bool> GetIsWiredAudioMicMuteSoundEnableAsync(string Guid);
        Task<int> GetWiredAudioVolumeAdjustmentToneAsync(string Guid);
        Task<bool> GetIsWiredAudioIMicNSEnableAsync(string Guid);
        Task<bool> GetIsAudioEqualizerSupportedAsync(string Guid);

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
        #endregion
    }
}