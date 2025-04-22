#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// IDPeMPlugin.cs created on 10/4/2022T3:37 PM
//

#endregion

using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.PluginConditions;
using DPeMPublic.Common.Enums;
using System;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public class OSDEventArgs
    {
        public string Requester { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public OSDType osd_type;
        public OSDType_Device osd_device;
        public string Message { get; set; } = string.Empty;
        public Guid Guid { get; set; } = default;
        public bool Status { get; set; } = false;
    }

    public interface IDPeMPlugin : IFrameworkPlugin
    {
        Task<PluginCondition> GetDPeMPluginConditionAsync();

        event EventHandler<DeviceChangedEventArgs> Notify;

        event EventHandler<bool> UpdateNotify;

        event EventHandler<Tuple<string, string>> OverlayNotify;

        event EventHandler<CollaborationMsg> CollaborationMsgNotify;

        event EventHandler<bool> IsZoomCallbacksRegisteredChanged;

        event EventHandler<bool> IsZoomMultipleCallsDetectedChanged;

        event EventHandler<bool> CollabMultipleCallsDetectedChanged;

        void NotifyNow();

        Task<DeviceHelper> GetDevices(bool Rescan = false);
        Task<DeviceHelper> GetDevices_WithoutAwait(bool Rescan = false);

        Task<CTKMessageHelper> GetCTKMessageHelper();

        Task<RFDeviceHelper> GetRFDongleDevices();

        Task<ClientInfo> GetDPeMClientInfo();

        void DisplayNotification(string bannerInfo, string hyperlinkText, string bannerItemType);

        Task<UpdateItemInfo> GetDPeMAssemblyUpdateInfo();

        Task<UpdateHelper> GetFWUpdateInfo();
        //Bruce, FWU need it
        Task<int> GetIODongleCountGen3AgoCount();

        void SetDPIValue(int newDPIValue, Guid deviceId);

        void SetDPILevel(int newDPILevel, Guid deviceId);

        //void SetPrimaryMouseButton(MouseButton newMouseButton, Guid deviceId);

        void SetTouchScrollSensitivityLevel(int newTouchScrollSensitivityLevel, Guid deviceId);

        void SetCollaborationKeyEnable(bool newValue, Guid deviceId);

        void SetCollaborationCameraEnable(bool newValue, Guid deviceId);

        void SetCollaborationScreenShareEnable(bool newValue, Guid deviceId);

        void SetCollaborationChatEnable(bool newValue, Guid deviceId);

        void SetCollaborationMicEnable(bool newValue, Guid deviceId);

        void SetCollaborationBlinkEffectEnable(bool newValue, Guid deviceId);

        void SetCollaborationDoubleTapEnable(bool newValue, Guid deviceId);

        void SetBackLightingControls(int newValue, Guid deviceId);

        void SetBackLightingLevel(int newValue, Guid deviceId);

        void CheckForUpdate();

        void StartPairing(Guid physicalDeviceId);
        void StopPairing(Guid physicalDeviceId);

        void UnPair(Guid logicalDeviceId);

        void SetWiredAudioIMicNSEnable(bool newValue, Guid deviceId);

        void SetWiredAudioMicMuteSoundEnable(bool newValue, Guid deviceId);

        void SetWiredAudioVolumeAdjustmentTone(int newValue, Guid deviceId);

        void SetAncMode(int newValue, Guid deviceId);

        void SetAncGain(int newValue, Guid deviceId);

        void SetSelectedPreset(int newValue, Guid deviceId);

        void SetBandsGain(int newValue, Guid deviceId, string bandGainNumber);

        void SetMicNoiseCancellation(bool newValue, Guid deviceId);

        void SetMicNoiseCancellationForMito(bool newValue, Guid deviceId);

        void SetSidetone(bool newValue, Guid deviceId);

        void SetSidetoneLevel(int newValue, Guid deviceId);

        void SetWearDetection(int newValue, Guid deviceId);

        void SetWearDetectionForCLI(int newValue, Guid deviceId);

        void SetBusyLight(bool newValue, Guid deviceId);

        void SetVoiceGuidance(bool newValue, Guid deviceId);

        void SetMicNCIncoming(bool newValue, Guid deviceId);

        void SetIsMicEnumerationOn(bool newValue, Guid deviceId);

        void SetCurrentSelectedProfile(string newValue, Guid deviceId);

        void SetSideTopSwitchSinglePressSetting(byte[] newValue, Guid deviceId);

        // webcam presence detection
        void SetWALTime(int newValue, Guid deviceId);
        void SetSnooze(int newValue, Guid deviceId);
        void SetSnoozeLength(int newValue, Guid deviceId);
        //void SetIsProximitySensorEnable(bool newValue, Guid deviceId);
        void SetIsWakeonApproachEnable(bool newValue, Guid deviceId);
        void SetIsWalkAwayLockEnable(bool newValue, Guid deviceId);

        int GetSnooze(Guid deviceId);
        int GetSnoozeLength(Guid deviceId);
        Task<bool> StartCopilotRegistryMonitor();
        Task<bool> StopCopilotRegistryMonitor();

        void UpdateDTPInstance(IDTPProxyPlugin DTPInstance);
        void UpdateSettingsInstance(ISettingsManagerDev SettingsInstance);
        event EventHandler<OSDEventArgs> Peripheral_OSD_Notify;

        Task UpdateLowBatteryOSD(bool showOSD);
    }

    public interface IDPeMServiceRegPlugin : IFrameworkPlugin
    {
        IDPeMPlugin DPeMPlugin { get; }
    }
}