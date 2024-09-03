#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// PeripheralPlugin.cs created on 30/4/2022T3:37 PM
//

#endregion

using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using DPeMPublic.Common.Enums;
using IndiLogic.DPeM.Broker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using IDeviceManager = IndiLogic.DPeM.Broker.IDeviceManager;

namespace DDPM.SA.Plugins.PeripheralsPlugin
{
    [Plugin(IDs.DDPM_PERIPHERALS_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IDPeMPlugin) })]
    public class PeripheralsPlugin : BaseAgentPlugin, IDPeMPlugin
    {
        private object _PeripheralLock = new object();

        #region Properties and fields

        private const string pluginName = "PeripheralsPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements DDPM Peripherals Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements DDPM Peripherals Plugin.";

        private readonly IAgent _agent;
        public const string PluginLogId = "Peripherals";

        private DeviceHelper _deviceHelper;
        private UpdateHelper _updateHelper;
        private RFDeviceHelper _rfDeviceHelper;
        private ClientInfo _clientInfo;
        private bool IsPhysicalDeviceEventAdded = false;

        #endregion

        #region Private Members

        private IClient _iClient;
        private IDeviceManager _iDeviceManager;
        private IUpdateManager _iUpdateManager;
        private IOverlayManager _iOverlayManager;
        private ICTKMessageHelper _iCTKMessageHelper;

        public bool UpdateAvailable { get; set; }

        private UpdateItemInfo _updateItems = new();

        private static List<Guid> PhysicalDevices1 = new();
        private static List<Guid> PhysicalDevices2 = new();
        private static List<Guid> LogicalDevices1 = new();
        private static List<Guid> LogicalDevices2 = new();
        private static List<Guid> LogicalDevices3 = new();
        private static List<Guid> LogicalDevices4 = new();
        private static List<Guid> LogicalDevicesPen = new();

        #endregion

        public IUpdateManager IUpdateManager => _iUpdateManager;
        public IOverlayManager IOverlayManager => _iOverlayManager;
        public ICTKMessageHelper ICTKMessageHelper => _iCTKMessageHelper;

        public IDeviceManager IDeviceManager => _iDeviceManager;

        #region Constructor

        public PeripheralsPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            IndiLogic.DPeM.Broker.Client.StatusEvent += Client_StatusEvent;
            IndiLogic.DPeM.Broker.Client.StartImpersonator();
        }

        #endregion

        #region IDPeM implementation

        public event EventHandler<DeviceChangedEventArgs> Notify;

        public event EventHandler<bool> UpdateNotify;

        public event EventHandler<Tuple<string, string>> OverlayNotify;

        public event EventHandler<CollaborationMsg> CollaborationMsgNotify;

        public event EventHandler<bool> IsZoomCallbacksRegisteredChanged;

        public event EventHandler<bool> IsZoomMultipleCallsDetectedChanged;

        public event EventHandler<bool> CollabMultipleCallsDetectedChanged;

        public void NotifyNow()
        {
            OnNotify(new DeviceChangedEventArgs());
        }

        public Task<PluginCondition> GetDPeMPluginConditionAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<DeviceHelper> GetDevices()
        {
            ScanDevices();
            if (_deviceHelper != null)
            {
                return await Task.Run(() => _deviceHelper);
            }
            return new DeviceHelper();
        }

        public async Task<CTKMessageHelper> GetCTKMessageHelper()
        {
            var CTKMessageHelper = new CTKMessageHelper();
            CTKMessageHelper.CollaborationMsg = _iCTKMessageHelper.CollaborationMsg.ToString();
            CTKMessageHelper.IsCollabMultipleCallsDetected = _iCTKMessageHelper.IsCollabMultipleCallsDetected;
            CTKMessageHelper.IsZoomCallbacksRegistered = _iCTKMessageHelper.IsZoomCallbacksRegistered;
            CTKMessageHelper.IsZoomClientInstalled = _iCTKMessageHelper.IsZoomClientInstalled;
            CTKMessageHelper.IsZoomMultipleCallsDetected = _iCTKMessageHelper.IsZoomMultipleCallsDetected;
            CTKMessageHelper.IsZoomVersionSupported = _iCTKMessageHelper.IsZoomVersionSupported;
            CTKMessageHelper.TeamsSDKState = _iCTKMessageHelper.TeamsSDKState.ToString();

            return await Task.Run(() => CTKMessageHelper);
        }

        public async Task<RFDeviceHelper> GetRFDongleDevices()
        {
            if (_rfDeviceHelper != null)
            {
                ScanDevices();
                return await Task.Run(() => _rfDeviceHelper);
            }
            return new RFDeviceHelper();
        }

        public async Task<ClientInfo> GetDPeMClientInfo()
        {
            if (_clientInfo != null)
            {
                return await Task.Run(() => _clientInfo);
            }
            return new ClientInfo();
        }

        public void SetDPILevel(int newDPILevel, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.DpiLevel != newDPILevel)
                    {
                        _logicalDevice3.SetDPILevel(newDPILevel);
                        _deviceInfo.DpiLevel = newDPILevel;
                        break;
                    }
                }
            }
        }

        public void SetDPIValue(int newDPIValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice2 _logicalDevice2)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.DpiValue != newDPIValue.ToString())
                    {
                        _logicalDevice2.SetDPIValue(newDPIValue);
                        _deviceInfo.DpiValue = newDPIValue.ToString();
                        break;
                    }
                }
            }
        }

        public void SetPrimaryMouseButton(MouseButton newMouseButton, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.MousePrimaryButton != newMouseButton)
                    {
                        _logicalDevice3.SetPrimaryMouseButton(newMouseButton);
                        _deviceInfo.MousePrimaryButton = newMouseButton;
                        break;
                    }
                }
            }
        }

        public void SetTouchScrollSensitivityLevel(int newTouchScrollSensitivityLevel, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.TouchScrollSensitivityLevel != newTouchScrollSensitivityLevel)
                    {
                        _logicalDevice3.SetTouchScrollSensitivityLevel(newTouchScrollSensitivityLevel);
                        _deviceInfo.TouchScrollSensitivityLevel = newTouchScrollSensitivityLevel;
                        break;
                    }
                }
            }
        }

        public void SetCollaborationKeyEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.IsCollaborationKeyEnable != newValue)
                    {
                        _logicalDevice3.SetCollaborationKeyEnable(newValue);
                        _deviceInfo.IsCollaborationKeyEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetCollaborationCameraEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.IsCollaborationCameraEnable != newValue)
                    {
                        _logicalDevice3.SetCollaborationCameraEnable(newValue);
                        _deviceInfo.IsCollaborationCameraEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetCollaborationScreenShareEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.IsCollaborationScreenShareEnable != newValue)
                    {
                        _logicalDevice3.SetCollaborationScreenShareEnable(newValue);
                        _deviceInfo.IsCollaborationScreenShareEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetCollaborationChatEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.IsCollaborationChatEnable != newValue)
                    {
                        _logicalDevice3.SetCollaborationChatEnable(newValue);
                        _deviceInfo.IsCollaborationChatEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetCollaborationMicEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.IsCollaborationMicEnable != newValue)
                    {
                        _logicalDevice3.SetCollaborationMicEnable(newValue);
                        _deviceInfo.IsCollaborationMicEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetCollaborationBlinkEffectEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.IsCollaborationBlinkEffectEnable != newValue)
                    {
                        _logicalDevice3.SetCollaborationBlinkEffectEnable(newValue);
                        _deviceInfo.IsCollaborationBlinkEffectEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetCollaborationDoubleTapEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.IsCollaborationDoubleTapEnable != newValue)
                    {
                        _logicalDevice3.SetCollaborationDoubleTapEnable(newValue);
                        _deviceInfo.IsCollaborationDoubleTapEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetBackLightingControls(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.BackLightingControls != newValue)
                    {
                        _logicalDevice3.SetBackLightingControls(newValue);
                        _deviceInfo.BackLightingControls = newValue;
                        break;
                    }
                }
            }
        }

        public void SetBackLightingLevel(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null && _deviceInfo.BackLightingLevel != newValue)
                    {
                        _logicalDevice3.SetBackLightingLevel(newValue);
                        _deviceInfo.BackLightingLevel = newValue;
                        break;
                    }
                }
            }
        }

        public void StartPairing(Guid physicalDeviceId)
        {
            if (_iDeviceManager != null && _iDeviceManager.Devices.Count > 0)
            {
                var physicalDevice = _iDeviceManager.Devices.FirstOrDefault(x => x.Id == physicalDeviceId);
                if (physicalDevice is IPhysicalDeviceDongle _physicalDeviceDongle)
                {
                    _physicalDeviceDongle.StartPairing();
                }
                else if (physicalDevice is IPhysicalAudioDeviceDongle _physicalAudioDeviceDongle)
                {
                    _physicalAudioDeviceDongle.StartPairing();
                }
            }
        }

        public void StopPairing(Guid physicalDeviceId)
        {
            if (_iDeviceManager != null && _iDeviceManager.Devices.Count > 0)
            {
                var physicalDevice = _iDeviceManager.Devices.FirstOrDefault(x => x.Id == physicalDeviceId);
                if (physicalDevice is IPhysicalDeviceDongle _physicalDeviceDongle)
                {
                    _physicalDeviceDongle.StopPairing();
                }
                else if (physicalDevice is IPhysicalAudioDeviceDongle _physicalAudioDeviceDongle)
                {
                    _physicalAudioDeviceDongle.StopPairing();
                }
            }
        }

        public void UnPair(Guid logicalDeviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == logicalDeviceId);
                if (logicalDevice != null)
                {
                    if (logicalDevice.ParentPhysicalDevice is IPhysicalDeviceDongle _logicalDeviceDongle)
                    {
                        _logicalDeviceDongle.UnPair(logicalDeviceId.ToString());
                        break;
                    }
                    else if (logicalDevice.ParentPhysicalDevice is IPhysicalAudioDeviceDongle _logicalAudioDeviceDongle)
                    {
                        _logicalAudioDeviceDongle.UnPair(logicalDeviceId.ToString());
                        break;
                    }
                }
            }
        }

        public void SetWiredAudioIMicNSEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalWiredAudio _logicalWiredAudioDevice)
                {
                    _logicalWiredAudioDevice.SetWiredAudioIMicNSEnable(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.IsWiredAudioIMicNSEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetWiredAudioMicMuteSoundEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalWiredAudio _logicalWiredAudioDevice)
                {
                    _logicalWiredAudioDevice.SetWiredAudioMicMuteSoundEnable(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.IsWiredAudioMicMuteSoundEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetWiredAudioVolumeAdjustmentTone(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalWiredAudio _logicalWiredAudioDevice)
                {
                    _logicalWiredAudioDevice.SetWiredAudioVolumeAdjustmentTone(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.WiredAudioVolumeAdjustmentTone = newValue;
                        break;
                    }
                }
            }
        }

        public void SetSidetoneLevel(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    _logicalDeviceHeadset.SetSidetoneLevel(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.SidetoneLevel = newValue;
                        break;
                    }
                }
            }
        }

        public void SetAncMode(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    _logicalDeviceHeadset.SetAncMode(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.AncMode = newValue;
                        break;
                    }
                }
            }
        }

        public void SetAncGain(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    _logicalDeviceHeadset.SetAncGain(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.AncGain = newValue;
                        break;
                    }
                }
            }
        }

        public void SetSelectedPreset(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    _logicalDeviceHeadset.SetSelectedPreset(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.SelectedPreset = newValue;
                        break;
                    }
                }
            }
        }

        public void SetBandsGain(int newValue, Guid deviceId, string bandGainNumber)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        switch (bandGainNumber)
                        {
                            case "band1gain":
                                _deviceInfo.Band1Gain = newValue;
                                break;

                            case "band2gain":
                                _deviceInfo.Band2Gain = newValue;
                                break;

                            case "band3gain":
                                _deviceInfo.Band3Gain = newValue;
                                break;

                            case "band4gain":
                                _deviceInfo.Band4Gain = newValue;
                                break;

                            case "band5gain":
                                _deviceInfo.Band5Gain = newValue;
                                break;
                        }
                        var bandGainNewValue = SetBandsGainValue(_logicalDeviceHeadset, _deviceInfo);
                        _logicalDeviceHeadset.SetBandsGain(bandGainNewValue);
                        break;
                    }
                }
            }
        }

        public void SetMicNoiseCancellation(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    _logicalDeviceHeadset.SetMicNoiseCancellation(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.MicNoiseCancellation = newValue;
                        break;
                    }
                }
            }
        }

        public void SetSidetone(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    _logicalDeviceHeadset.SetSidetone(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.Sidetone = newValue;
                        break;
                    }
                }
            }
        }

        public void SetWearDetection(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    _logicalDeviceHeadset.SetWearDetection(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.WearDetection = newValue;
                        break;
                    }
                }
            }
        }

        public void SetBusyLight(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    _logicalDeviceHeadset.SetBusyLight(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.BusyLight = newValue;
                        break;
                    }
                }
            }
        }

        public void SetVoiceGuidance(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    _logicalDeviceHeadset.SetVoiceGuidance(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.VoiceGuidance = newValue;
                        break;
                    }
                }
            }
        }

        public void SetMicNCIncoming(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    _logicalDeviceHeadset.SetMicNCIncoming(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.MicNCIncoming = newValue;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Webcam methods

        public void SetIsMicEnumerationOn(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    _iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.IsMicEnumerationOn = newValue;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            Log.Info("Enter DPeMTask1 ");

            PluginCondition = new PluginStartedCondition();
        }

        #endregion

        #region Private Methods

        private void ScanDevices()
        {
            _deviceHelper = new DeviceHelper
            {
                deviceInfo = new List<DeviceInfo>()
            };
            _deviceHelper.DPeMSDKVersion = Assembly.GetAssembly(typeof(ILogicalDevice)).GetName().Version.ToString();
            _deviceHelper.DCFVersion = Assembly.GetAssembly(typeof(PluginOrderGroupType)).GetName().Version.ToString();
            _deviceHelper.DPeMSubAgentVersion = Assembly.GetAssembly(typeof(PeripheralsPlugin)).GetName().Version.ToString();

            _rfDeviceHelper = new RFDeviceHelper
            {
                dongleInfo = new List<DongleInfo>()
            };

            //Robert_Lin, workaround to avoid _iDeviceManager==null
            if (_iDeviceManager == null)
                return;

            //_deviceHelper.DPeMSDKVersion = IndiLogic.DPeM.Broker.Assembly.GetName();
            foreach (var device in _iDeviceManager.Devices)
            {
                FillRFDeviceInfo(device);
                // << 240712 fix empty dongle no event issue by Hess
                if (device.Type == DeviceType.PhysicalDongle && device is IPhysicalDeviceDongle physicalDeviceDongle && !PhysicalDevices2.Contains(device.Id))
                {
                    physicalDeviceDongle.PairedDeviceCountChanged += IPhysicalDeviceDongle_PairedDeviceCountChanged;
                    physicalDeviceDongle.PairingStatusChanged += IPhysicalDeviceDongle_PairingStatusChanged;
                    PhysicalDevices2.Add(device.Id);
                }
                if (device.Type == DeviceType.PhysicalAudioDongle && device is IPhysicalAudioDeviceDongle physicalAudioDeviceDongle && !PhysicalDevices2.Contains(device.Id))
                {
                    physicalAudioDeviceDongle.PairedDeviceCountChanged += PhysicalAudioDeviceDongle_PairedDeviceCountChanged;
                    physicalAudioDeviceDongle.PairingStatusChanged += PhysicalAudioDeviceDongle_PairingStatusChanged;
                    PhysicalDevices2.Add(device.Id);
                }
                // >>

                if (device.Type == DeviceType.PhysicalPen)
                {
                    _deviceHelper.IsdDriverVersion = ((IPhysicalPenDevice)device).IsdServiceVersion;
                }

                foreach (var item in device.Devices)
                {
                    DeviceInfo info = new()
                    {
                        ID = item.Id,
                        PhyscialDeviceID = item.ParentPhysicalDevice.Id,
                        Name = item.Name,
                        BatteryLevel = item.BatteryLevel,
                        BatteryStatus = item.BatteryStatus.ToString(),
                        FirmwareVersion = item.FirmwareVersion.ToString("X4"),
                        PhysicalDeviceFirmwareVersion = item.ParentPhysicalDevice.FirmwareVersion.ToString("X4"),
                        DeviceImage = item.ThumbnailImageRawData,
                        IsConnected = true,
                        InterfaceType = item.InterfaceType,
                        Type = item.Type,
                        PluginId = item.PluginId,
                        OdmId = item.ODMId,
                        ModelNumber = item.ModelNumber,
                        IsBatteryLevelSupported = item.IsBatteryLevelSupported,
                        ThumbnailImageRawData = item.ThumbnailImageRawData,
                    };

                    if (item.ParentPhysicalDevice.Type == DeviceType.PhysicalDongle)
                    {
                        if (item.ParentPhysicalDevice is IPhysicalDeviceDongle _physicalDeviceDongle)
                        {
                            info.PairingStatusName = UpdateParingStausText(_physicalDeviceDongle.PairingStatus);
                            info.MaxPairingSlots = _physicalDeviceDongle.MaxPairingSlots;
                            info.PairedDeviceCount = _physicalDeviceDongle.PairedDeviceCount;
                            info.IsPhysicalDeviceDongle = true; //Because if it is a physical device dongle it will return true.
                        }
                    }

                    if (item.ParentPhysicalDevice.Type == DeviceType.PhysicalAudioDongle)
                    {
                        if (item.ParentPhysicalDevice is IPhysicalAudioDeviceDongle _physicalAudioDeviceDongle)
                        {
                            info.PairingStatusName = UpdateParingStausText(_physicalAudioDeviceDongle.PairingStatus);
                            info.MaxPairingSlots = _physicalAudioDeviceDongle.MaxPairingSlots;
                            info.PairedDeviceCount = _physicalAudioDeviceDongle.PairedDeviceCount;
                            info.IsPhysicalDeviceDongle = false;
                        }
                    }

                    if (item.ParentPhysicalDevice.Type == DeviceType.PhysicalPen)
                    {
                        if (item.ParentPhysicalDevice is IPhysicalPenDevice physicalDevicePen)
                        {
                            var pen = (ILogicalDevicePen)item;
                            info.IsdDriverVersion = physicalDevicePen.IsdDriverVersion;
                            info.IsdServiceVersion = physicalDevicePen.IsdServiceVersion;
                            info.TiltSensitivity = pen.TiltSensitivity;
                            info.TipSensitivity = pen.TipSensitivity;
                            info.EraserDoublePressSetting = pen.EraserDoublePressSetting;
                            info.EraserDoublePressValues = pen.EraserDoublePressValues;
                            info.EraserLongPressSetting = pen.EraserLongPressSetting;
                            info.EraserLongPressValues = pen.EraserLongPressValues;
                            info.EraserSinglePressSetting = pen.EraserSinglePressSetting;
                            info.EraserSinglePressValues = pen.EraserSinglePressValues;
                            info.IsBLE = pen.IsBLE;
                            info.IsSideBottomButtonHoverClick = pen.IsSideBottomButtonHoverClick;
                            info.IsSideTopButtonHoverClick = pen.IsSideTopButtonHoverClick;
                            info.LaunchableAppValues = pen.LaunchableAppValues;
                            info.MenuSinglePressSetting = pen.MenuSinglePressSetting;
                            info.MenuSinglePressValues = pen.MenuSinglePressValues;
                            info.MenuCenterRightClickSetting = pen.MenuCenterRightClickSetting;
                            info.SideBottomSwitchSinglePressSetting = pen.SideBottomSwitchSinglePressSetting;
                            info.SideSwitchSinglePressValues = pen.SideSwitchSinglePressValues;
                            info.SideTopSwitchSinglePressSetting = pen.SideTopSwitchSinglePressSetting;
                            physicalDevicePen.IsdVersionChanged += IPhysicalDevicePen_IsdVersionChanged;
                            if (!LogicalDevicesPen.Contains(pen.Id))
                            {
                                pen.PenSettingChanged += Pen_PenSettingChanged;
                                LogicalDevicesPen.Add(pen.Id);
                            }
                        }
                        Debug.Write($"TiltSensitivity: {info.TiltSensitivity}");
                    }

                    if (item is ILogicalDevice2 _logicalDevice2)
                    {
                        info.IsCollabsKeysSupported = _logicalDevice2.IsCollabsKeysSupported;
                        info.CollabsKeysSupported = _logicalDevice2.IsCollabsKeysSupported ? "Supported" : "Not Supported";
                        info.DpiValue = _logicalDevice2.DpiLevelValue.ToString();
                        info.PairedHostName1 = _logicalDevice2.PairedHostName1;
                        info.PairedHostName2 = _logicalDevice2.PairedHostName2;
                        info.PairedHostName3 = _logicalDevice2.PairedHostName3;
                        info.LogicalDeviceType = _logicalDevice2.Type.ToString();
                        info.PhysicalDeviceType = device.Type;
                        info.TotalNumberOfPairedHostName = _logicalDevice2.TotalNumberOfPaiedHostName;
                        info.IsDPILevelSupported = _logicalDevice2.IsDPILevelSupported;

                        if (!LogicalDevices1.Contains(_logicalDevice2.Id))
                        {
                            _logicalDevice2.DpiLevelChanged += ILogicalDevice_DpiLevelChanged;
                            LogicalDevices1.Add(_logicalDevice2.Id);
                        }
                    }

                    if (item is ILogicalDevice3 _logicalDevice3)
                    {
                        info.IsCollaborationBlinkEffectEnable = _logicalDevice3.IsCollaborationBlinkEffectEnable();
                        info.IsCollaborationCameraEnable = _logicalDevice3.IsCollaborationCameraEnable();
                        info.IsCollaborationChatEnable = _logicalDevice3.IsCollaborationChatEnable();
                        info.IsCollaborationDoubleTapEnable = _logicalDevice3.IsCollaborationDoubleTapEnable();
                        info.IsCollaborationKeyEnable = _logicalDevice3.IsCollaborationKeyEnable();
                        info.IsCollaborationMicEnable = _logicalDevice3.IsCollaborationMicEnable();
                        info.IsCollaborationScreenShareEnable = _logicalDevice3.IsCollaborationScreenShareEnable();
                        info.IsIlluminationSupported = _logicalDevice3.IsIlluminationSupported;
                        info.BackLightingControls = _logicalDevice3.BackLightingControls;
                        info.BackLightingLevel = _logicalDevice3.BackLightingLevel;
                        info.IsTouchScrollSensitivitySupported = _logicalDevice3.IsTouchScrollSensitivitySupported;
                        info.IsReportRateSupported = _logicalDevice3.IsReportRateSupported;
                        info.TouchScrollSensitivityLevel = _logicalDevice3.TouchScrollSensitivityLevel;
                        info.ReportRate = _logicalDevice3.ReportRate;
                        info.IsDPIValueSupported = _logicalDevice3.IsDPIValueSupported;
                        info.IsDPILevelChangePending = _logicalDevice3.IsDPILevelChangePending;
                        info.IsDPIValueChangePending = _logicalDevice3.IsDPIValueChangePending;
                        info.DpiLevel = _logicalDevice3.DpiLevel;
                        info.DpiLevelValues = _logicalDevice3.DpiLevelValues;
                        info.DpiMin = _logicalDevice3.DpiMin;
                        info.DpiMax = _logicalDevice3.DpiMax;
                        info.DpiDelta = _logicalDevice3.DpiDelta;
                        info.InstanceNumber = _logicalDevice3.InstanceNumber;
                        info.InstanceId = _logicalDevice3.InstanceId;
                        info.ColorCode = _logicalDevice3.ColorCode;
                        info.MousePrimaryButton = _logicalDevice3.MousePrimaryButton;
                        //info.PairedHostNames = _logicalDevice3.PairedHostNames;

                        if (!LogicalDevices2.Contains(_logicalDevice3.Id))
                        {
                            _logicalDevice3.MousePrimaryButtonChanged += ILogicalDevice_MousePrimaryButtonChanged;
                            _logicalDevice3.DPIValueChanged += ILogicalDevice_DpiValueChanged;
                            _logicalDevice3.TouchScrollSensitivityLevelChanged += ILogicalDevice_TouchScrollSensitivityLevelChanged;
                            _logicalDevice3.BackLightingControlsChanged += ILogicalDevice_BackLightingControlsChanged;
                            _logicalDevice3.BackLightingLevelChanged += ILogicalDevice_BackLightingLevelChanged;
                            LogicalDevices2.Add(_logicalDevice3.Id);
                        }
                    }

                    if (item is ILogicalWiredAudio _logicalWiredAudio)
                    {
                        info.MuteStatus = _logicalWiredAudio.MuteStatus;
                        info.IsWiredAudioIMicNSEnable = _logicalWiredAudio.IsWiredAudioIMicNSEnable();
                        info.IsWiredAudioMicMuteSoundEnable = _logicalWiredAudio.IsWiredAudioMicMuteSoundEnable();
                        info.WiredAudioVolumeAdjustmentTone = _logicalWiredAudio.GetWiredAudioVolumeAdjustmentTone();
                        _logicalWiredAudio.MuteStatusChanged += ILogicalWiredAudio_MuteStatusChanged;
                    }

                    if (item is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                    {
                        info.DeviceSymbolicLink = _iLogicalDeviceWebcam.DeviceSymbolicLink;
                        info.ParentDevInstanceId = _iLogicalDeviceWebcam.ParentDevInstanceId;
                        info.IsESISupported = _iLogicalDeviceWebcam.IsESISupported;
                        info.SupportedProperties = _iLogicalDeviceWebcam.SupportedProperties;
                        info.FOVValues = _iLogicalDeviceWebcam.FOVValues;
                        info.SupportedResolutions = _iLogicalDeviceWebcam.SupportedResolutions;
                        info.SupportedFeatures = _iLogicalDeviceWebcam.SupportedFeatures;
                        info.CurrentFeatures = _iLogicalDeviceWebcam.CurrentFeatures;
                        info.IsMicEnumerationSupported = _iLogicalDeviceWebcam.IsMicEnumerationSupported;
                        info.IsMicEnumerationOn = _iLogicalDeviceWebcam.IsMicEnumerationOn;
                        info.IsWindowsHelloSupported = _iLogicalDeviceWebcam.IsWindowsHelloSupported;
                        info.HasWindowsHelloPowerConstraint = _iLogicalDeviceWebcam.HasWindowsHelloPowerConstraint;
                        info.IsWindowsHelloSupported = _iLogicalDeviceWebcam.IsWindowsHelloSupported;
                        _iLogicalDeviceWebcam.IsMicEnumerationOnChanged += _iLogicalDeviceWebcam_IsMicEnumerationOnChanged;
                    }

                    if (item is ILogicalDeviceHeadset _logicalDeviceHeadset)
                    {
                        info.IsReady = _logicalDeviceHeadset.IsReady;
                        info.IsDirty = _logicalDeviceHeadset.IsDirty;
                        info.IsMicNoiseCancellationSupported = _logicalDeviceHeadset.IsMicNoiseCancellationSupported;
                        info.IsSidetoneSupported = _logicalDeviceHeadset.IsSidetoneSupported;
                        info.IsBusyLightSupported = _logicalDeviceHeadset.IsBusyLightSupported;
                        info.IsVoiceGuidanceSupported = _logicalDeviceHeadset.IsVoiceGuidanceSupported;
                        info.IsPresetsSupported = _logicalDeviceHeadset.IsPresetsSupported;
                        info.IsEqualizerSupported = _logicalDeviceHeadset.IsEqualizerSupported;
                        info.ConnectionType = _logicalDeviceHeadset.ConnectionType;
                        info.IsANCSupported = _logicalDeviceHeadset.IsANCSupported;
                        info.AncMode = _logicalDeviceHeadset.AncMode;
                        info.AncGain = _logicalDeviceHeadset.AncGain;
                        info.WearDetection = _logicalDeviceHeadset.WearDetection;
                        info.IsMicNCIncomingSupported = _logicalDeviceHeadset.IsMicNCIncomingSupported;
                        info.IsWearDetectionSupported = _logicalDeviceHeadset.IsWearDetectionSupported; //Visibile in Peagsus/Mito
                        info.IsWearDetectionSensitivitySupported = _logicalDeviceHeadset.IsWearDetectionSensitivitySupported; //Visibile in Peagsus/Mito
                        info.IsWearDetectionPauseMusicSupported = _logicalDeviceHeadset.IsWearDetectionPauseMusicSupported; //Visibile in Peagsus/Mito
                        info.IsWearDetectionMuteMicSupported = _logicalDeviceHeadset.IsWearDetectionMuteMicSupported; //Visibile in Peagsus/Mito
                        info.IsWearDetectionQuickPauseSupported = _logicalDeviceHeadset.IsWearDetectionQuickPauseSupported; //Visibile in Mito
                        info.IsWearDetectionChecked = (info.WearDetection & 0x1) != 0 ? true : false;
                        info.IsPauseMusicChecked = (info.WearDetection & 0x2) != 0 ? true : false;
                        info.IsMuteMicrophoneChecked = (info.WearDetection & 0x4) != 0 ? true : false;
                        info.IsQuickPauseChecked = (info.WearDetection & 0x10) != 0 || (info.WearDetection & 0x20) != 0 ? true : false;

                        info.MicNoiseCancellation = _logicalDeviceHeadset.MicNoiseCancellation;
                        info.MicNCIncoming = _logicalDeviceHeadset.MicNCIncoming;
                        info.Sidetone = _logicalDeviceHeadset.Sidetone;
                        info.BusyLight = _logicalDeviceHeadset.BusyLight;
                        info.VoiceGuidance = _logicalDeviceHeadset.VoiceGuidance;
                        info.SelectedPreset = _logicalDeviceHeadset.SelectedPreset;
                        info.SidetoneLevel = _logicalDeviceHeadset.SidetoneLevel;
                        info.MuteStatus = _logicalDeviceHeadset.MuteStatus;
                        info.BandsGain = _logicalDeviceHeadset.BandsGain;
                        SetEqualizerValues(_logicalDeviceHeadset, info);

                        _logicalDeviceHeadset.IsReadyChanged += _logicalDeviceHeadset_IsReadyChanged;
                        _logicalDeviceHeadset.IsDirtyChanged += _logicalDeviceHeadset_IsDirtyChanged;
                        _logicalDeviceHeadset.MicNoiseCancellationChanged += _logicalDeviceHeadset_MicNoiseCancellationChanged;
                        _logicalDeviceHeadset.MicNCIncomingChanged += _logicalDeviceHeadset_MicNCIncomingChanged;
                        _logicalDeviceHeadset.SidetoneChanged += _logicalDeviceHeadset_SidetoneChanged;
                        _logicalDeviceHeadset.BusyLightChanged += _logicalDeviceHeadset_BusyLightChanged;
                        _logicalDeviceHeadset.VoiceGuidanceChanged += _logicalDeviceHeadset_VoiceGuidanceChanged;
                        _logicalDeviceHeadset.SelectedPresetChanged += _logicalDeviceHeadset_SelectedPresetChanged;
                        _logicalDeviceHeadset.SidetoneLevelChanged += _logicalDeviceHeadset_SidetoneLevelChanged;
                        _logicalDeviceHeadset.MuteStatusChanged += _logicalDeviceHeadset_MuteStatusChanged;
                        _logicalDeviceHeadset.BandsGainChanged += _logicalDeviceHeadset_BandsGainChanged;
                        _logicalDeviceHeadset.AncModeChanged += _logicalDeviceHeadset_AncModeChanged;
                        _logicalDeviceHeadset.AncGainChanged += _logicalDeviceHeadset_AncGainChanged;
                        _logicalDeviceHeadset.WearDetectionChanged += _logicalDeviceHeadset_WearDetectionChanged;
                    }

                    if (item is ILogicalDeviceDock _logicalDeviceDock)
                    {
                        info.MonitorCount = _logicalDeviceDock.MonitorCount;
                        info.DockInfo = _logicalDeviceDock.DockInfo;
                        info.DockType = _logicalDeviceDock.DockType;
                        info.DockServiceTag = _logicalDeviceDock.DockServiceTag;
                        info.FirmwareVersion = _logicalDeviceDock.DockPackageFwVersion;
                        info.DockPackageFwVersion = _logicalDeviceDock.DockPackageFwVersion;
                        info.DockFwUpdateStatus = _logicalDeviceDock.DockFwUpdateStatus;
                        info.DockTBTConnectionStatus = _logicalDeviceDock.DockTBTConnectionStatus;
                        try
                        {
                            string textString = System.Text.Encoding.UTF8.GetString(_logicalDeviceDock.DockData);
                            Debug.WriteLine(textString);
                            DockData dockData = JsonSerializer.Deserialize<DockData>(textString);
                            info.DockData = dockData;
                            info.ModelNumber = dockData.MarketingName;
                            info.Name = $"Dell Dock {dockData.MarketingName}";
                            if (info.ModelNumber.ToUpper().StartsWith("WD19S"))
                            {
                                info.Name = $"Dell Dock {dockData.MarketingName}_{dockData.PowerSupplyWattage}W";
                            }
                            if (string.IsNullOrEmpty(info.DockServiceTag))
                            {
                                info.DockServiceTag = dockData.ServiceTag;
                            }
                            if (string.IsNullOrEmpty(info.FirmwareVersion) || info.FirmwareVersion.StartsWith("0000"))
                            {
                                info.FirmwareVersion = dockData.PackageFirmwareVersion.ToString("X4");
                            }
                        }
                        catch
                        {
                        }
                    }
                    _deviceHelper.deviceInfo.Add(info);

                    //item.update
                }
                if (!IsPhysicalDeviceEventAdded)
                {
                    _iDeviceManager_DeviceAddedEvent(device);
                    IsPhysicalDeviceEventAdded = true;
                }
            }

            Console.WriteLine(_deviceHelper.ToString());
        }

        private void Pen_PenSettingChanged(ILogicalDevicePen arg1, string arg2)
        {
            Console.WriteLine(arg2);
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "PenSettingChanged";
                OnNotify(_EventArgs);
            }
        }

        private void FillRFDeviceInfo(IPhysicalDevice device)
        {
            if (device is IPhysicalAudioDeviceDongle _iPhysicalAudioDeviceDongle)
            {
                DongleInfo rfInfo = new DongleInfo();
                var rfdongle = _rfDeviceHelper.dongleInfo.Where(x => x.DeviceType == _iPhysicalAudioDeviceDongle.Type).FirstOrDefault();
                if (rfdongle != null)
                {
                    rfdongle.IsMultipleDongleFound = true;
                }
                else
                {
                    rfInfo = new DongleInfo()
                    {
                        ID = _iPhysicalAudioDeviceDongle.Id,
                        DeviceType = _iPhysicalAudioDeviceDongle.Type,
                        IsMultipleDongleFound = false,
                        MaxPairingSlots = _iPhysicalAudioDeviceDongle.MaxPairingSlots,
                        PairedDeviceCount = _iPhysicalAudioDeviceDongle.PairedDeviceCount,
                        LogicalDeviceIDs = _iPhysicalAudioDeviceDongle.Devices.Select(x => x.Id).ToList()
                    };
                    _rfDeviceHelper.dongleInfo.Add(rfInfo);
                }
            }
            else if (device is IPhysicalDeviceDongle _iPhysicalDeviceDongle)
            {
                DongleInfo rfInfo = new DongleInfo();
                var rfdongle = _rfDeviceHelper.dongleInfo.Where(x => x.DeviceType == _iPhysicalDeviceDongle.Type).FirstOrDefault();
                if (rfdongle != null)
                {
                    rfdongle.IsMultipleDongleFound = true;
                }
                else
                {
                    rfInfo = new DongleInfo()
                    {
                        ID = _iPhysicalDeviceDongle.Id,
                        DeviceType = _iPhysicalDeviceDongle.Type,
                        IsMultipleDongleFound = false,
                        MaxPairingSlots = _iPhysicalDeviceDongle.MaxPairingSlots,
                        PairedDeviceCount = _iPhysicalDeviceDongle.PairedDeviceCount,
                        LogicalDeviceIDs = _iPhysicalDeviceDongle.Devices.Select(x => x.Id).ToList()
                    };
                    _rfDeviceHelper.dongleInfo.Add(rfInfo);
                }
            }
        }

        // >>

        private string UpdateParingStausText(DonglePairingStatus donglePairingStatus)
        {
            switch (donglePairingStatus)
            {
                case DonglePairingStatus.DonglePairingStatusStopped:
                    return "Stopped";

                case DonglePairingStatus.DonglePairingStatusStarted:
                    return "Started";

                case DonglePairingStatus.DonglePairingStatusRequest:
                    return "Request";

                case DonglePairingStatus.DonglePairingStatusTimeOut:
                    return "TimeOut";

                case DonglePairingStatus.DonglePairingStatusAlreadyPaired:
                    return "Already Paired";

                case DonglePairingStatus.DonglePairingStatusOldDevice:
                    return "Old Device";
            }
            return "";
        }

        private string UpdateParingStausText(AudioDonglePairingStatus donglePairingStatus)
        {
            switch (donglePairingStatus)
            {
                case AudioDonglePairingStatus.AudioDonglePairingStatusStopped:
                    return "Stopped";

                case AudioDonglePairingStatus.AudioDonglePairingStatusStarted:
                    return "Started";

                case AudioDonglePairingStatus.AudioDonglePairingStatusRequest:
                    return "Request";

                case AudioDonglePairingStatus.AudioDonglePairingStatusTimeOut:
                    return "TimeOut";

                case AudioDonglePairingStatus.AudioDonglePairingStatusAlreadyPaired:
                    return "Already Paired";
            }
            return "";
        }

        public void SetEqualizerValues(ILogicalDeviceHeadset logicalDeviceHeadset, DeviceInfo info)
        {
            byte[] Band1 = new byte[4];
            byte[] Band2 = new byte[4];
            byte[] Band3 = new byte[4];
            byte[] Band4 = new byte[4];
            byte[] Band5 = new byte[4];

            byte[] bandsGain = logicalDeviceHeadset.BandsGain;

            if (bandsGain != null && bandsGain.Length >= 20)
            {
                Array.Copy(bandsGain, 0 * 4, Band1, 0, 4);
                Array.Copy(bandsGain, 1 * 4, Band2, 0, 4);
                Array.Copy(bandsGain, 2 * 4, Band3, 0, 4);
                Array.Copy(bandsGain, 3 * 4, Band4, 0, 4);
                Array.Copy(bandsGain, 4 * 4, Band5, 0, 4);
            }

            info.Band1Gain = ByteArrayToInt(Band1);
            info.Band2Gain = ByteArrayToInt(Band2);
            info.Band3Gain = ByteArrayToInt(Band3);
            info.Band4Gain = ByteArrayToInt(Band4);
            info.Band5Gain = ByteArrayToInt(Band5);
        }

        private byte[] SetBandsGainValue(ILogicalDeviceHeadset logicalDeviceHeadset, DeviceInfo info)
        {
            // Create a new BandsGain array or use the existing one
            //byte[] bandsGain = logicalDeviceHeadset.BandsGain ?? new byte[20];
            byte[] bandsGain = new byte[20];//force set new byte[]

            // Update BandsGain array with the current values of Band1Gain to Band5Gain
            IntToByteArray(info.Band1Gain, bandsGain, 0);
            IntToByteArray(info.Band2Gain, bandsGain, 4);
            IntToByteArray(info.Band3Gain, bandsGain, 8);
            IntToByteArray(info.Band4Gain, bandsGain, 12);
            IntToByteArray(info.Band5Gain, bandsGain, 16);

            // Optionally set the BandsGain array back to the logicalDeviceHeadset
            info.BandsGain = bandsGain;

            // Return the updated BandsGain array
            return bandsGain;
        }

        private void IntToByteArray(int value, byte[] byteArray, int startIndex)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Array.Copy(bytes, 0, byteArray, startIndex, 4);
        }

        public static int ByteArrayToInt(byte[] bandgain)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bandgain);

            int i = BitConverter.ToInt32(bandgain, 0);
            return i;
        }

        #endregion

        #region EventHandlers

        private void OnNotify(DeviceChangedEventArgs e)
        {
            if (Notify != null)
                Notify(this, e);
        }

        private void OnUpdateNotify(bool isUpdateAvailable)
        {
            UpdateNotify?.Invoke(this, isUpdateAvailable);
        }

        private void OnOverlayNotify(bool arg1, string arg2, string arg3)
        {
            OverlayNotify?.Invoke(_iOverlayManager, new Tuple<string, string>(arg2, arg3));
        }

        private void OnCollaborationMsgNotify(CollaborationMsg collaborationMsg)
        {
            CollaborationMsgNotify?.Invoke(_iOverlayManager, collaborationMsg);
        }

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e?.ChangedPlugins == null || !e.ChangedPlugins.Any())
            {
            }
        }

        private void Client_StatusEvent(ClientStatus status, IClient client)
        {
            if (status == ClientStatus.Connected)
            {
                _iClient = client;
                _iDeviceManager = _iClient.DeviceManager;
                _iDeviceManager.DeviceAddedEvent += _iDeviceManager_DeviceAddedEvent;
                _iDeviceManager.DeviceRemovedEvent += _iDeviceManager_DeviceRemovedEvent;
                ScanDevices();

                _iUpdateManager = _iClient.UpdateManager;
                _iUpdateManager.IsAnyUpdateAvailableChanged += IUpdateManager_IsAnyUpdateAvailableChanged;

                _iOverlayManager = _iClient.OverlayManager;
                _iOverlayManager.VolatileSettingsChanged += _iOverlayManager_VolatileSettingsChanged;

                _iCTKMessageHelper = _iClient.CTKMessageHelper;
                _iCTKMessageHelper.CollaborationMsgChanged += _iCTKMessageHelper_CollaborationMsgChanged;
                _iCTKMessageHelper.CollabMultipleCallsDetectedChanged += _iCTKMessageHelper_CollabMultipleCallsDetectedChanged;
                _iCTKMessageHelper.IsZoomMultipleCallsDetectedChanged += _iCTKMessageHelper_IsZoomMultipleCallsDetectedChanged;
                _iCTKMessageHelper.IsZoomCallbacksRegisteredChanged += _iCTKMessageHelper_IsZoomCallbacksRegisteredChanged;
            }
            else
            {
                _iDeviceManager = null;
                _iUpdateManager = null;
                _iClient = null;
            }

            //
            // Update UI with client and service status
            //
            _clientInfo = new ClientInfo
            {
                status = status.ToString()
            };

            if (status == ClientStatus.Connected)
            {
                if (_iClient != null)
                {
                    _clientInfo.ServiceStatus = _iClient.ServiceStatus.ToString();
                    _clientInfo.ApiVersion = _iClient.APIVersion.ToString();
                }
            }
            else
            {
                _clientInfo.ServiceStatus = "Unknown";
                _clientInfo.ApiVersion = "Unknown";
            }
            Console.WriteLine(_clientInfo.ToString());
        }

        private void _iCTKMessageHelper_IsZoomCallbacksRegisteredChanged(bool obj)
        {
            IsZoomCallbacksRegisteredChanged.Invoke(this, obj);
        }

        private void _iCTKMessageHelper_IsZoomMultipleCallsDetectedChanged(bool obj)
        {
            IsZoomMultipleCallsDetectedChanged.Invoke(this, obj);
        }

        private void _iCTKMessageHelper_CollabMultipleCallsDetectedChanged(bool obj)
        {
            CollabMultipleCallsDetectedChanged.Invoke(this, obj);
        }

        private void _iCTKMessageHelper_CollaborationMsgChanged(CollaborationMsg collaborationMsg)
        {
            CollaborationMsgNotify?.Invoke(EventArgs.Empty, collaborationMsg);
        }

        private void _iOverlayManager_VolatileSettingsChanged(bool arg1, string arg2, string arg3)
        {
            OverlayNotify?.Invoke(EventArgs.Empty, new Tuple<string, string>(arg2, arg3));
        }

        private void _iDeviceManager_DeviceAddedEvent(IPhysicalDevice iPhysicalDevice)
        {
            System.Diagnostics.Debug.WriteLine("ParentPhysicalDevice Added, Id : " + iPhysicalDevice.Id + ", Name : " + iPhysicalDevice.Name);

            iPhysicalDevice.DeviceAddedEvent += IPhysicalDevice_DeviceAddedEvent;
            iPhysicalDevice.DeviceRemovedEvent += IPhysicalDevice_DeviceRemovedEvent;
        }

        private void _iDeviceManager_DeviceRemovedEvent(IPhysicalDevice iPhysicalDevice)
        {
            lock (_PeripheralLock)
            {
                iPhysicalDevice.DeviceRemovedEvent -= IPhysicalDevice_DeviceRemovedEvent;
                _iDeviceManager = _iClient?.DeviceManager;
                System.Diagnostics.Debug.WriteLine("ParentPhysicalDevice Removed, Id : " + iPhysicalDevice.Id + ", Name : " + iPhysicalDevice.Name);
                if (PhysicalDevices1.Contains(iPhysicalDevice.Id))
                {
                    PhysicalDevices1.Remove(iPhysicalDevice.Id);

                    if (iPhysicalDevice.Type == DeviceType.PhysicalAudioDongle || iPhysicalDevice.Type == DeviceType.PhysicalDongle)
                    {
                        DeviceChangedEventArgs _EventArgs = new();
                        _EventArgs.type = DeviceChangedType.Peripherals_UnPlug;
                        _EventArgs.changedProperty = "PhysicalDeviceRemoved";
                        OnNotify(_EventArgs);
                    }
                }
            }
        }

        private void IPhysicalDevice_DeviceAddedEvent(ILogicalDevice iLogicalDevice)
        {
            System.Diagnostics.Debug.WriteLine("LogicalDevice Added, Id : " + iLogicalDevice.Id + ", Name : " + iLogicalDevice.Name);
            if (!LogicalDevices4.Contains(iLogicalDevice.Id))
            {
                iLogicalDevice.BatteryStatusChanged += ILogicalDevice_BatteryStatusChanged;
                iLogicalDevice.BatteryLevelChanged += ILogicalDevice_BatteryLevelChanged;
                LogicalDevices4.Add(iLogicalDevice.Id);
            }

            ScanDevices();

            DeviceChangedEventArgs _EventArgs = new();
            _EventArgs.type = DeviceChangedType.Peripherals_PlugIn;
            _EventArgs.device_peripherals = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == iLogicalDevice.Id.ToString());
            _EventArgs.changedProperty = "LogicalDeviceAdded";
            OnNotify(_EventArgs);
        }

        private void IPhysicalDevice_DeviceRemovedEvent(ILogicalDevice iLogicalDevice)
        {
            _deviceHelper.deviceInfo.Where(x => x.ID == iLogicalDevice.Id).ToList().ForEach(device =>
            {
                device.IsConnected = false;

                DeviceChangedEventArgs _EventArgs = new()
                {
                    type = DeviceChangedType.Peripherals_UnPlug,
                    device_peripherals = device,
                    changedProperty = "LogicalDeviceRemoved"
                };
                OnNotify(_EventArgs);
            });
            if (LogicalDevices1.Contains(iLogicalDevice.Id))
            {
                LogicalDevices1.Remove(iLogicalDevice.Id);
            }
            if (LogicalDevices2.Contains(iLogicalDevice.Id))
            {
                LogicalDevices2.Remove(iLogicalDevice.Id);
            }
            if (LogicalDevices3.Contains(iLogicalDevice.Id))
            {
                LogicalDevices3.Remove(iLogicalDevice.Id);
            }
            if (LogicalDevices4.Contains(iLogicalDevice.Id))
            {
                LogicalDevices4.Remove(iLogicalDevice.Id);
            }
            if (LogicalDevicesPen.Contains(iLogicalDevice.Id))
            {
                LogicalDevicesPen.Remove(iLogicalDevice.Id);
            }
            Console.WriteLine(_deviceHelper.ToString());
        }

        private void ILogicalDevice_DpiLevelChanged(ILogicalDevice2 arg1, int arg2)
        {
            Console.WriteLine(arg2.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Console.WriteLine(arg2.ToString());
                if (deviceInfo == null)
                    return;
                if (arg2 == 0)
                    return;
                deviceInfo.DpiLevel = arg2 - 1;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "DpiLevelChanged";
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalDevice_DpiValueChanged(ILogicalDevice3 arg1, int arg2)
        {
            Console.WriteLine(arg2.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Console.WriteLine(arg2.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.DpiValue = arg2.ToString();
                }
                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "DpiValueChanged";
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalDevice_TouchScrollSensitivityLevelChanged(ILogicalDevice3 arg1, int arg2)
        {
            Console.WriteLine(arg2.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Console.WriteLine(arg2.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.TouchScrollSensitivityLevel = arg2;
                    deviceInfo.TouchSensitivityLevelValue = deviceInfo.TouchScrollSensitivityLevel switch
                    {
                        1 => 100,
                        2 => 50,
                        _ => 0
                    };
                }
                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "TouchScrollSensitivityLevelChanged";
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalDevice_BackLightingControlsChanged(ILogicalDevice3 arg1, int arg2)
        {
            Console.WriteLine(arg2.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Console.WriteLine(arg2.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.BackLightingControls = arg2;
                    deviceInfo.BackLightTabIndex = deviceInfo.BackLightingControls switch
                    {
                        1 => 0,
                        3 => 2,
                        6 => 1,
                        _ => 0
                    };
                }
                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "BackLightingControlsChanged";
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalDevice_BackLightingLevelChanged(ILogicalDevice3 arg1, int arg2)
        {
            Console.WriteLine(arg2.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Console.WriteLine(arg2.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.BackLightingLevel = arg2;

                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "BackLightingLevelChanged";
                    OnNotify(_EventArgs);
                }
            }
        }

        private void ILogicalDevice_BatteryStatusChanged(ILogicalDevice arg1, BatteryStatus arg2)
        {
            Console.WriteLine(arg2.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Console.WriteLine(arg2.ToString());
                if (deviceInfo != null)
                    deviceInfo.BatteryStatus = arg2.ToString();

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "BatteryStatusChanged";
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalDevice_BatteryLevelChanged(ILogicalDevice arg1, int arg2)
        {
            Console.WriteLine(arg2.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.BatteryLevel = arg2;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "BatteryLevelChanged";
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalWiredAudio_MuteStatusChanged(ILogicalWiredAudio arg1, bool newMuteStatus)
        {
            Console.WriteLine(newMuteStatus.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.MuteStatus = newMuteStatus;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "MuteStatusChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_IsReadyChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.IsReady = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "IsReadyChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_IsDirtyChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.IsDirty = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "IsDirtyChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_MicNoiseCancellationChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.MicNoiseCancellation = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "MicNoiseCancellationChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_MicNCIncomingChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.MicNCIncoming = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "MicNCIncomingChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_BusyLightChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.BusyLight = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "BusyLightChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_VoiceGuidanceChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.VoiceGuidance = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "VoiceGuidanceChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_MuteStatusChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.MuteStatus = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "MuteStatusChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_SidetoneChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.Sidetone = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "SidetoneChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_SelectedPresetChanged(ILogicalDeviceHeadset logicalDeviceHeadset, int newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.SelectedPreset = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "SelectedPresetChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_SidetoneLevelChanged(ILogicalDeviceHeadset logicalDeviceHeadset, int newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.SidetoneLevel = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "SidetoneLevelChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_BandsGainChanged(ILogicalDeviceHeadset logicalDeviceHeadset, byte[] newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.BandsGain = newValue;
                SetEqualizerValues(logicalDeviceHeadset, deviceInfo);

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "BandsGainChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_AncModeChanged(ILogicalDeviceHeadset logicalDeviceHeadset, int newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.AncMode = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "AncModeChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_AncGainChanged(ILogicalDeviceHeadset logicalDeviceHeadset, int newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.AncGain = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "AncGainChanged";
                OnNotify(_EventArgs);
            }
        }

        private void _logicalDeviceHeadset_WearDetectionChanged(ILogicalDeviceHeadset logicalDeviceHeadset, int newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.WearDetection = newValue;

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "WearDetectionChanged";
                OnNotify(_EventArgs);
            }
        }

        private void IUpdateManager_IsAnyUpdateAvailableChanged(bool isAnyUpdateAvailable)
        {
            _updateHelper = new UpdateHelper();
            UpdateAvailable = isAnyUpdateAvailable;
            _updateHelper.UpdateItems = new List<UpdateItemInfo>();
            foreach (var updateItem in _iUpdateManager.AllUpdateItems)
            {
                _updateItems = new UpdateItemInfo() { UpdateType = updateItem.Type.ToString(), UpdateSeverity = updateItem.Severity.ToString(), NewVersion = updateItem.NewVersion, Description = updateItem.Description };

                _updateItems.CurrentVersion = updateItem.CurrentVersion;
                _updateItems.DeviceId = updateItem.DeviceId;
                _updateItems.DeviceIndex = updateItem.DeviceIndex;
                _updateItems.DeviceModelNumber = updateItem.DeviceModelNumber;
                _updateItems.DeviceName = updateItem.DeviceName;
                _updateItems.DevicePath = updateItem.DevicePath;
                _updateItems.DeviceType = updateItem.DeviceType;
                _updateItems.FrimwareUpdatePath = updateItem.FrimwareUpdatePath;
                _updateItems.InstallPath = updateItem.InstallPath;
                _updateItems.InstanceId = updateItem.InstanceId;
                _updateItems.Priority = updateItem.Priority;
                _updateItems.ServerPath = updateItem.ServerPath;
                _updateItems.SupplierID = updateItem.SupplierID;

                _updateHelper.UpdateItems.Add(_updateItems);
            }

            Console.WriteLine(isAnyUpdateAvailable ? "UpdateAvailable" : "Already Updated.");
            if (isAnyUpdateAvailable)
            {
                OnUpdateNotify(isAnyUpdateAvailable);
            }
        }

        private void IPhysicalDeviceDongle_PairedDeviceCountChanged(IPhysicalDeviceDongle physicalDeviceDongle, int newValue)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == physicalDeviceDongle.Id.ToString());
                Console.WriteLine(newValue.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.PairedDeviceCount = newValue;

                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "DonglePairedDeviceCountChanged";
                    OnNotify(_EventArgs);
                    //ScanDevices();
                }
            }
        }

        private void IPhysicalDeviceDongle_PairingStatusChanged(IPhysicalDeviceDongle physicalDeviceDongle, int newPairingStatus, int dongleDeviceType, string requestDeviceName)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == physicalDeviceDongle.Id.ToString());
                // << 240712 fix empty dongle issue by Hess
                if (deviceInfo == null)
                    deviceInfo = new DeviceInfo();
                // >>
                {
                    deviceInfo.PairingStatusName = UpdateParingStausText((DonglePairingStatus)newPairingStatus);

                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = $"DonglePairingStatusChanged|{requestDeviceName}";
                    OnNotify(_EventArgs);
                }
            }
        }

        private void PhysicalAudioDeviceDongle_PairingStatusChanged(IPhysicalAudioDeviceDongle arg1, AudioDonglePairingStatus arg2)
        {
            throw new NotImplementedException();
        }

        private void PhysicalAudioDeviceDongle_PairedDeviceCountChanged(IPhysicalAudioDeviceDongle arg1, int arg2)
        {
            //var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == arg1.Id.ToString());
            Console.WriteLine(arg2.ToString());

            DeviceChangedEventArgs _EventArgs = new();
            _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
            _EventArgs.device_peripherals = new()
            {
                ID = arg1.Id,
                PairedDeviceCount = arg2,
                IsPhysicalDeviceDongle = true,
                PhysicalDeviceType = arg1.Type
            };
            _EventArgs.changedProperty = "DonglePairedDeviceCountChanged";
            OnNotify(_EventArgs);
        }

        private void IPhysicalDevicePen_IsdVersionChanged(IPhysicalPenDevice physicalPenDevice, string newValue)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == physicalPenDevice.Id.ToString());
                Console.WriteLine(newValue);
                if (deviceInfo != null)
                {
                    deviceInfo.IsdDriverVersion = newValue;

                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "PenVersionChanged";
                    OnNotify(_EventArgs);
                }
            }
        }

        private void _iLogicalDeviceWebcam_IsMicEnumerationOnChanged(ILogicalDeviceWebcam iLogicalDeviceWebcam, bool newValue)
        {
        }

        private void ILogicalDevice_MousePrimaryButtonChanged(ILogicalDevice3 logicalDevice3, MouseButton newValue)
        {
            Console.WriteLine(newValue.ToString());

            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDevice3.Id.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.MousePrimaryButton = newValue;

                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "MousePrimaryButtonChanged";
                    OnNotify(_EventArgs);
                }
            }
        }

        public async Task<UpdateItemInfo> GetDPeMAssemblyUpdateInfo()
        {
            return await Task.Run(() =>
            {
                if (_updateItems == null)
                    return new UpdateItemInfo();
                Console.WriteLine(Convert.ToString(_updateItems.NewVersion));
                return _updateItems;
            });
        }

        public async Task<UpdateHelper> GetFWUpdateInfo()
        {
            if (_updateHelper != null)
            {
                return await Task.Run(() => _updateHelper);
            }
            return await Task.Run(() => new UpdateHelper());
        }

        public void DisplayNotification(string bannerInfo, string hyperlinkText, string bannerItemType)
        {
        }

        public void CheckForUpdate()
        {
            if (_iUpdateManager != null)
            {
                _iUpdateManager.CheckForUpdate();
            }
        }

        #endregion
    }
}