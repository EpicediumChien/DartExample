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
        Task<int> GetDpiValue(string itemID);

        Task SetDPIValue(string itemID, int newValue);

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
        Task<JArray> GetPresetProfiles(string Guid);
        Task<JArray> GetCustomProfiles(string Guid);
        Task<string> GetProfile(string Guid);
        Task<string> GetProfileName(string Guid);
        Task<int> GetBrightness(string Guid);

        Task<string> GetCameraFirmwareVersion(string itemID);

        Task<bool> CheckIsPropertyFOVSupported(string itemID);

        Task<int> GetFieldOfViewValue(string itemID);

        Task<bool> CheckIsPropertyHDRSupported(string itemID);

        Task<bool> GetIsHDROnValue(string itemID);

        Task SetIsHDROnValue(string itemID, bool newValue);

        Task<bool> CheckIsPropertyAntiFlickerSupported(string itemID);

        Task<int> GetAntiFlickerValue(string itemID);

        Task SetAntiFlickerValue(string itemID, int newValue);

        Task<bool> CheckIsPropertyAutoFramingSupported(string itemID);

        Task<bool> GetIsAutoFramingOnValue(string itemID);

        Task SetIsAutoFramingOnValue(string itemID, bool newValue);
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

        #endregion

        #region Headset

        Task SetMicNoiseCancellationAsync(string Guid, bool newValue);
        Task SetSidetoneAsync(string Guid, bool newValue);
        Task SetBusyLightAsync(string Guid, bool newValue);
        Task SetVoiceGuidanceAsync(string Guid, bool newValue);
        Task SetSelectedPresetAsync(string Guid, int newValue);
        Task SetSidetoneLevelAsync(string Guid, int newValue);
        Task SetBandsGainAsync(string Guid, byte[] newValue);
        Task SetAncModeAsync(string Guid, int newValue);
        Task SetAncGainAsync(string Guid, int newValue);
        Task SetWearDetectionAsync(string Guid, int newValue);
        Task SetMicNCIncomingAsync(string Guid, bool newValue);
        Task SetUnPairAsync(string Guid, bool newValue);
        Task SetFactoryResetAsyncValueForHeadset(string Guid, bool newValue);

        //////////Headset Get//////////

        Task<JArray> GetDeviceItemsExAsync(string Guid);
        //Task<DeviceInterfaceType> GetInterfaceTypeAsync(string Guid);
        Task<string> GetDeviceNameAsync(string Guid);
        Task<string> GetDeviceIdAsync(string Guid);
        Task<string> GetPluginIdAsync(string Guid);
        Task<int> GetODMIdAsync(string Guid);
        Task<string> GetModelNumberAsync(string Guid);
        Task<int> GetInstanceNumberAsync(string Guid);
        Task<int> GetInstanceIdAsync(string Guid);
        Task<string> GetFirmwareVersionAsync(string Guid);
        Task<string> GetDeviceTypeAsync(string Guid);
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
        //Task<HeadsetConnectionType> GetConnectionTypeAsync(string Guid);
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
        Task<int> GetAncModeAsync(string Guid);
        Task<int> GetAncGainAsync(string Guid);
        Task<int> GetWearDetectionAsync(string Guid);
        Task<bool> GetIsMicNCIncomingSupportedAsync(string Guid);

        #endregion

        #region Wired Audio

        Task<int> GetBassAsync(string guid);
        Task SetBassAsync(string guid, int newValue);
        Task<int> GetMidRangeAsync(string guid);
        Task SetMidRangeAsync(string guid, int newValue);
        Task<int> GetTrebleAsync(string guid);
        Task SetTrebleAsync(string guid, int newValue);
        Task SetIsWiredAudioMicMuteSoundEnableAsync(string guid, bool newValue);
        Task<bool> GetIsWiredAudioMicMuteSoundEnableAsync(string itemID);
        Task SetWiredAudioVolumeAdjustmentToneAsync(string guid, int newValue);
        Task<int> GetWiredAudioVolumeAdjustmentToneAsync(string itemID);
        Task SetIsWiredAudioIMicNSEnableAsync(string itemID, bool newValue);
        Task<bool> GetIsWiredAudioIMicNSEnableValueAsync(string itemID);
        Task SetResetToDefaultAsyncForSoundbar(string itemID, bool newValue);

        #endregion
    }
}