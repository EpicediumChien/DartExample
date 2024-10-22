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
using Newtonsoft.Json.Linq;
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


        Task SetDpiValue(string Guid, int newValue);
        Task SetMouseAction(string Guid, byte[] newValue);
        Task SetCurrentSelectedAppSpecificProfile(string Guid, string newValue);
        Task DeleteMouseAssignedAction(string Guid, int newValue);

        Task SetMsAssignDialogAction(string Guid, string newValue);
        Task SetMsAssignKeystrokeAction(string Guid, string newValue);

        #endregion

        #region Keyboard

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

        Task<string> GetCameraFirmwareVersion(string Guid);

        Task<bool> GetIsPropertyFOVSupported(string Guid);

        Task<int> GetFieldOfView(string Guid);

        Task<bool> GetIsPropertyHDRSupported(string Guid);

        Task<bool> GetIsHDROn(string Guid);

        Task<bool> CheckIsPropertyAntiFlickerSupported(string Guid);

        Task<int> GetAntiFlicker(string Guid);

        Task<bool> GetIsPropertyAutoFramingSupported(string Guid);

        Task<bool> GetIsAutoFramingOn(string Guid);

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

        Task SetFactoryResetAsyncValueForHeadset(string Guid, bool newValue);

        #endregion

        #region Wired Audio

        Task<int> GetBassAsync(string Guid);
        Task SetBassAsync(string Guid, int newValue);
        Task<int> GetMidRangeAsync(string Guid);
        Task SetMidRangeAsync(string Guid, int newValue);
        Task<int> GetTrebleAsync(string Guid);
        Task SetTrebleAsync(string Guid, int newValue);
        Task SetIsWiredAudioMicMuteSoundEnableAsync(string Guid, bool newValue);
        Task<bool> GetIsWiredAudioMicMuteSoundEnableAsync(string Guid);
        Task SetWiredAudioVolumeAdjustmentToneAsync(string Guid, int newValue);
        Task<int> GetWiredAudioVolumeAdjustmentToneAsync(string Guid);
        Task SetIsWiredAudioIMicNSEnableValue(string Guid, bool newValue);
        Task<bool> GetIsWiredAudioIMicNSEnableValueAsync(string Guid);
        Task SetResetToDefaultValueAsync(string Guid, bool newValue);

        #endregion
    }
}