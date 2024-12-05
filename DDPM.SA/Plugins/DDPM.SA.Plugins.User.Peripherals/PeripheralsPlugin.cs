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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using VcpCore.Common;
using IDeviceManager = IndiLogic.DPeM.Broker.IDeviceManager;
using IDs = DDPM.SA.Common.IDs;

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
        private const string publisherCompany = "Dell Inc.";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements DDPM Peripherals Plugin.";

        private readonly IAgent _agent;
        public const string PluginLogId = "Peripherals";

        private DeviceHelper _deviceHelper;
        private UpdateHelper _updateHelper;
        //Bruce, FWU need it
        private int _DongleCount;
        private RFDeviceHelper _rfDeviceHelper;
        private ClientInfo _clientInfo;
        private static Logs _logs;
        #endregion

        #region Private Members

        private IClient _iClient;
        private IDeviceManager _iDeviceManager;
        private IUpdateManager _iUpdateManager;
        private IOverlayManager _iOverlayManager;
        private ICTKMessageHelper _iCTKMessageHelper;
        private bool _isClientConnected;
        public bool UpdateAvailable { get; set; }

        private UpdateItemInfo _updateItems = new();

        private static List<Guid> PhysicalDevices = new();
        private static List<Guid> PhysicalDevices1 = new();
        private static List<Guid> PhysicalDevices2 = new();
        private static List<Guid> LogicalDevices1 = new();
        private static List<Guid> LogicalDevices2 = new();
        private static List<Guid> LogicalDevices3 = new();
        private static List<Guid> LogicalDevices4 = new();
        private static List<Guid> LogicalDevicesPen = new();

        private IDeviceManagerSA _DeviceManagerPlugin;
        private readonly object _PluginConditionLock_DeviceManager = new object();
        private readonly object _lock = new();

        #endregion

        public IUpdateManager IUpdateManager => _iUpdateManager;
        public IOverlayManager IOverlayManager => _iOverlayManager;
        public ICTKMessageHelper ICTKMessageHelper => _iCTKMessageHelper;

        public IDeviceManager IDeviceManager => _iDeviceManager;

        #region Constructor

        public PeripheralsPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _logs ??= new Logs(Log, PluginLogId);
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

        public async Task<DeviceHelper> GetDevices(bool Rescan = false)
        {
            if (Rescan)
                ScanDevices();

            if (_isClientConnected && _deviceHelper != null)
            {
                return await Task.Run(() =>
                {
                    lock (_lock)
                    {
                        if (_isClientConnected && _deviceHelper != null)
                        {
                            return _deviceHelper;
                        }
                        else
                        {
                            return new DeviceHelper
                            {
                                deviceInfo = new List<DeviceInfo>()
                            };

                        }
                    }
                });
            }

            return new DeviceHelper
            {
                deviceInfo = new List<DeviceInfo>()
            };
        }
        public Task<DeviceHelper> GetDevices_WithoutAwait(bool Rescan = false)
        {
            if (Rescan)
                ScanDevices();

            if (_deviceHelper != null)
            {
                return Task.FromResult(_deviceHelper);
            }
            return Task.FromResult(new DeviceHelper());
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
                        writelog($"SetDPILevel: Guid:{deviceId} NewValue: {newDPILevel}");
                        Debug.WriteLine($"SetDPILevel: Guid:{deviceId} NewValue: {newDPILevel}");
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
                        writelog($"SetDPIValue: Guid:{deviceId} NewValue: {newDPIValue}");
                        Debug.WriteLine($"SetDPIValue: Guid:{deviceId} NewValue: {newDPIValue}");
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
                        Debug.WriteLine($"SetBackLightingControls: GUID:{deviceId} NewValue:{newValue}");
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
                    else if (logicalDevice.ParentPhysicalDevice is IPhysicalPenDevice _physicalPenDevice)
                    {
                        _physicalPenDevice.UnPair(logicalDeviceId.ToString());
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
                    //_logicalWiredAudioDevice.SetWiredAudioIMicNSEnable(newValue);
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
                    //_logicalWiredAudioDevice.SetWiredAudioMicMuteSoundEnable(newValue);
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
                    //_logicalWiredAudioDevice.SetWiredAudioVolumeAdjustmentTone(newValue);
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
                        _logicalDeviceHeadset.SetBandsGain(_deviceInfo.Band1Gain, _deviceInfo.Band2Gain,
                           _deviceInfo.Band3Gain, _deviceInfo.Band4Gain, _deviceInfo.Band5Gain);

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
            //Elie. R17.1 drop this function. 1123
            //foreach (var device in _iDeviceManager.Devices)
            //{
            //    var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
            //    if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
            //    {
            //        _logicalDeviceHeadset.SetWearDetection(newValue);
            //        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
            //        if (_deviceInfo != null)
            //        {
            //            _deviceInfo.WearDetection = newValue;
            //            break;
            //        }
            //    }
            //}
        }
        public void SetWearDetectionForCLI(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                //Wayn R17.2 Change to DTP 12/2
                bool setvalue = (newValue == 1 ? true : false);
                _DeviceManagerPlugin.SetWearDetectionAsync(logicalDevice.ToString(), setvalue);
                //if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                //{
                //    if (newValue == 0)
                //        newValue |= 0b00000000;
                //    else
                //        newValue |= 0b00000111;
                //Elie. R17.1 drop this function. 1123
                //_logicalDeviceHeadset.SetWearDetection(newValue);
                //DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                //if (_deviceInfo != null)
                //{
                //    _deviceInfo.WearDetection = newValue;
                //    break;
                //}
                //}
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

        public void SetSideTopSwitchSinglePressSetting(byte[] newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevicePen _logicalDevicePen)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        var payloadBytes = (byte[])newValue;
                        var payloadSize = payloadBytes.Length;
                        var byteArray = new byte[payloadSize + 4];
                        BitConverter.GetBytes(payloadSize).CopyTo(byteArray, 0);
                        payloadBytes.CopyTo(byteArray, 4);
                        //_logicalDevicePen.SetSideTopSwitchSinglePressSetting(newValue);
                        _logicalDevicePen.SideTopSwitchSinglePressSetting = byteArray;
                        //_deviceInfo.SideTopSwitchSinglePressSetting = newValue;
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
                    Debug.WriteLine($"{newValue}");
                    _iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    //_iLogicalDeviceWebcam.IsMicEnumerationOn = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.IsMicEnumerationOn = newValue;
                        break;
                    }
                }
            }
        }

        public void SetCurrentSelectedProfile(string newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    Debug.WriteLine($"{newValue}");
                    _iLogicalDeviceWebcam.ProfileManager.SetCurrentSelectedProfile(newValue);
                }
            }
        }

        public void SetWALTime(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    Debug.WriteLine($"{newValue}");
                    //_iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    _iLogicalDeviceWebcam.WALTime = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.WALTime = newValue;
                        break;
                    }
                }
            }
        }

        public void SetSnooze(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    Debug.WriteLine($"{newValue}");
                    //_iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    _iLogicalDeviceWebcam.Snooze = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.Snooze = newValue;
                        break;
                    }
                }
            }
        }

        public void SetSnoozeLength(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    Debug.WriteLine($"{newValue}");
                    //_iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    //_iLogicalDeviceWebcam.SnoozeLength = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.SnoozeLength = newValue;
                        break;
                    }
                }
            }
        }

        public void SetIsProximitySensorEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    Debug.WriteLine($"{newValue}");
                    //_iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    _iLogicalDeviceWebcam.IsProximitySensorEnable = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.IsProximitySensorEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetIsWakeonApproachEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    Debug.WriteLine($"{newValue}");
                    //_iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    _iLogicalDeviceWebcam.IsWakeonApproachEnable = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.IsWakeonApproachEnable = newValue;
                        break;
                    }
                }
            }
        }

        public void SetIsWalkAwayLockEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    Debug.WriteLine($"{newValue}");
                    //_iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    _iLogicalDeviceWebcam.IsWalkAwayLockEnable = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.IsWalkAwayLockEnable = newValue;
                        break;
                    }
                }
            }
        }

        public int GetSnooze(Guid deviceId)
        {
            int nRes = -1;
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    //Debug.WriteLine($"{newValue}");
                    //_iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    //_iLogicalDeviceWebcam.WALTime = newValue;
                    nRes = _iLogicalDeviceWebcam.Snooze;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        //_deviceInfo.WALTime = newValue;
                        nRes = _deviceInfo.Snooze;
                        break;
                    }
                }
            }
            return nRes;
        }

        public int GetSnoozeLength(Guid deviceId)
        {
            int nRes = -1;
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    //Debug.WriteLine($"{newValue}");
                    //_iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    //_iLogicalDeviceWebcam.WALTime = newValue;
                    nRes = _iLogicalDeviceWebcam.SnoozeLength;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
                    if (_deviceInfo != null)
                    {
                        //_deviceInfo.WALTime = newValue;
                        nRes = _deviceInfo.SnoozeLength;
                        break;
                    }
                }
            }
            return nRes;
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            writelog("DTPProxyPlugin plugin starting");

            PluginCondition = new PluginStartedCondition();
            InitializeDeviceManagerPlugin();
        }

        #endregion

        #region Private Methods

        private void ScanDevices()
        {
            lock (_lock)
            {
                if (_isClientConnected && _iClient != null && _iDeviceManager != null)
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
                    //Bruce, FWU need it
                    _DongleCount = _iDeviceManager.Devices.ToList().FindAll(o => o.Type.Equals(DeviceType.PhysicalAudioDongle) || o.Type.Equals(DeviceType.PhysicalDongle)).Count;
                    _logs.DebugMsg_1("[PeripheralsPlugin] _DongleCount " + _DongleCount);
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
                            _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... Name " + item.Name);
                            _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... ModelNumber " + item.ModelNumber);
                            _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... BatteryLevel " + item.BatteryLevel);
                            _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... FirmwareVersion " + item.FirmwareVersion.ToString("X4"));
                            _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... PhysicalDeviceFirmwareVersion " + item.ParentPhysicalDevice.FirmwareVersion.ToString("X4"));
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
                                        pen.KeyCaptureStarted += Pen_KeyCaptureStarted;
                                        pen.KeyCaptureDataChanged += Pen_KeyCaptureDataChanged;
                                        pen.KeyCaptureProgressDataChanged += Pen_KeyCaptureProgressDataChanged;
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
                                    _logicalDevice3.PairedHostNameChanged += ILogicalDevice_PairedHostNameChanged;
                                    LogicalDevices2.Add(_logicalDevice3.Id);
                                }
                            }

                            if (item is ILogicalWiredAudio _logicalWiredAudio)
                            {
                                info.MuteStatus = _logicalWiredAudio.MuteStatus;
                                //info.IsWiredAudioIMicNSEnable = _logicalWiredAudio.IsWiredAudioIMicNSEnable();
                                //info.IsWiredAudioMicMuteSoundEnable = _logicalWiredAudio.IsWiredAudioMicMuteSoundEnable();
                                //info.WiredAudioVolumeAdjustmentTone = _logicalWiredAudio.GetWiredAudioVolumeAdjustmentTone();
                                _logicalWiredAudio.MuteStatusChanged += ILogicalWiredAudio_MuteStatusChanged;
                            }

                            if (item is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                            {
                                info.BrightnessMax = _iLogicalDeviceWebcam.BrightnessMax;
                                info.BrightnessMin = _iLogicalDeviceWebcam.BrightnessMin;
                                info.BrightnessSteppingDelta = _iLogicalDeviceWebcam.BrightnessSteppingDelta;
                                info.ContrastMax = _iLogicalDeviceWebcam.ContrastMax;
                                info.ContrastMin = _iLogicalDeviceWebcam.ContrastMin;
                                info.ContrastSteppingDelta = _iLogicalDeviceWebcam.ContrastSteppingDelta;
                                info.CurrentFeatures = _iLogicalDeviceWebcam.CurrentFeatures;
                                info.DeviceSymbolicLink = _iLogicalDeviceWebcam.DeviceSymbolicLink;
                                info.FocusMax = _iLogicalDeviceWebcam.FocusMax;
                                info.FocusMin = _iLogicalDeviceWebcam.FocusMin;
                                info.FocusSteppingDelta = _iLogicalDeviceWebcam.FocusSteppingDelta;
                                info.FOVValues = _iLogicalDeviceWebcam.FOVValues;
                                info.HasWindowsHelloPowerConstraint = _iLogicalDeviceWebcam.HasWindowsHelloPowerConstraint;
                                info.IsESISupported = _iLogicalDeviceWebcam.IsESISupported;
                                info.IsMicEnumerationOn = _iLogicalDeviceWebcam.IsMicEnumerationOn;
                                info.IsMicEnumerationSupported = _iLogicalDeviceWebcam.IsMicEnumerationSupported;
                                info.IsPropertyAntiFlickerSupported = _iLogicalDeviceWebcam.IsPropertyAntiFlickerSupported;
                                info.IsPropertyAutoFramingSensitivitySupported = _iLogicalDeviceWebcam.IsPropertyAutoFramingSensitivitySupported;
                                info.IsPropertyAutoFramingSizeSupported = _iLogicalDeviceWebcam.IsPropertyAutoFramingSizeSupported;
                                info.IsPropertyAutoFramingSupported = _iLogicalDeviceWebcam.IsPropertyAutoFramingSupported;
                                info.IsPropertyAutoFramingTransitionSupported = _iLogicalDeviceWebcam.IsPropertyAutoFramingTransitionSupported;
                                info.IsPropertyBrightnessSupported = _iLogicalDeviceWebcam.IsPropertyBrightnessSupported;
                                info.IsPropertyContrastSupported = _iLogicalDeviceWebcam.IsPropertyContrastSupported;
                                info.IsPropertyFocusSupported = _iLogicalDeviceWebcam.IsPropertyFocusSupported;
                                info.IsPropertyFOVSupported = _iLogicalDeviceWebcam.IsPropertyFOVSupported;
                                info.IsPropertyHDRSupported = _iLogicalDeviceWebcam.IsPropertyHDRSupported;
                                info.IsPropertyPanSupported = _iLogicalDeviceWebcam.IsPropertyPanSupported;
                                info.IsPropertyPrioritySupported = _iLogicalDeviceWebcam.IsPropertyPrioritySupported;
                                info.IsPropertySaturationSupported = _iLogicalDeviceWebcam.IsPropertySaturationSupported;
                                info.IsPropertySharpnessSupported = _iLogicalDeviceWebcam.IsPropertySharpnessSupported;
                                info.IsPropertyTiltSupported = _iLogicalDeviceWebcam.IsPropertyTiltSupported;
                                info.IsPropertyWhiteBalanceSupported = _iLogicalDeviceWebcam.IsPropertyWhiteBalanceSupported;
                                info.IsPropertyZoomSupported = _iLogicalDeviceWebcam.IsPropertyZoomSupported;
                                info.IsWindowsHelloSupported = _iLogicalDeviceWebcam.IsWindowsHelloSupported;
                                info.PanMax = _iLogicalDeviceWebcam.PanMax;
                                info.PanMin = _iLogicalDeviceWebcam.PanMin;
                                info.PanSteppingDelta = _iLogicalDeviceWebcam.PanSteppingDelta;
                                info.ParentDevInstanceId = _iLogicalDeviceWebcam.ParentDevInstanceId;
                                //info.ProfileManager = new(_iLogicalDeviceWebcam.ProfileManager);
                                info.PresetProfiles = JArray.FromObject(_iLogicalDeviceWebcam.ProfileManager.PresetProfiles);
                                info.CustomProfiles = JArray.FromObject(_iLogicalDeviceWebcam.ProfileManager.CustomProfiles);
                                info.Profile = _iLogicalDeviceWebcam.ProfileManager.CurrentSelectedProfile.Id;
                                info.ProfileDescription = _iLogicalDeviceWebcam.ProfileManager.CurrentSelectedProfile.Description;
                                info.ProfileName = _iLogicalDeviceWebcam.ProfileManager.CurrentSelectedProfile.Name;
                                info.SaturationMax = _iLogicalDeviceWebcam.SaturationMax;
                                info.SaturationMin = _iLogicalDeviceWebcam.SaturationMin;
                                info.SaturationSteppingDelta = _iLogicalDeviceWebcam.SaturationSteppingDelta;
                                info.SharpnessMax = _iLogicalDeviceWebcam.SharpnessMax;
                                info.SharpnessMin = _iLogicalDeviceWebcam.SharpnessMin;
                                info.SharpnessSteppingDelta = _iLogicalDeviceWebcam.SharpnessSteppingDelta;
                                info.SupportedFeatures = _iLogicalDeviceWebcam.SupportedFeatures;
                                info.SupportedProperties = _iLogicalDeviceWebcam.SupportedProperties;
                                info.SupportedResolutions = Encoding.UTF8.GetString(_iLogicalDeviceWebcam.SupportedResolutions);
                                info.TiltMax = _iLogicalDeviceWebcam.TiltMax;
                                info.TiltMin = _iLogicalDeviceWebcam.TiltMin;
                                info.TiltSteppingDelta = _iLogicalDeviceWebcam.TiltSteppingDelta;
                                info.WhiteBalanceMax = _iLogicalDeviceWebcam.WhiteBalanceMax;
                                info.WhiteBalanceMin = _iLogicalDeviceWebcam.WhiteBalanceMin;
                                info.WhiteBalanceSteppingDelta = _iLogicalDeviceWebcam.WhiteBalanceSteppingDelta;
                                info.ZoomMax = _iLogicalDeviceWebcam.ZoomMax;
                                info.ZoomMin = _iLogicalDeviceWebcam.ZoomMin;
                                info.ZoomSteppingDelta = _iLogicalDeviceWebcam.ZoomSteppingDelta;
                                _iLogicalDeviceWebcam.IsMicEnumerationOnChanged += _iLogicalDeviceWebcam_IsMicEnumerationOnChanged;

                                // webcam presence detection
                                info.Snooze = _iLogicalDeviceWebcam.Snooze;
                                info.SnoozeLength = _iLogicalDeviceWebcam.SnoozeLength;
                                info.IsProximitySensorEnable = _iLogicalDeviceWebcam.IsProximitySensorEnable;
                                info.IsWakeonApproachEnable = _iLogicalDeviceWebcam.IsWakeonApproachEnable;
                                info.IsWalkAwayLockEnable = _iLogicalDeviceWebcam.IsWalkAwayLockEnable;
                                info.WALTime = _iLogicalDeviceWebcam.WALTime;

                            }

                            if (item is ILogicalDeviceHeadset _logicalDeviceHeadset)
                            {
                                _logs.DebugMsg_1("[PeripheralsPlugin] ILogicalDeviceHeadset ... FirmwareVersion " + item.FirmwareVersion.ToString("X4"));
                                info.FirmwareVersion = item.FirmwareVersion.ToString("X4");
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
                                //Elie. R17.1 drop this function.1123
                                //info.WearDetection = _logicalDeviceHeadset.WearDetection;
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
                                //SetEqualizerValues(_logicalDeviceHeadset, info);

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
                                //Elie. R17.1 drop this function.1123
                                //_logicalDeviceHeadset.WearDetectionChanged += _logicalDeviceHeadset_WearDetectionChanged;
                            }

                            if (item is ILogicalDeviceDock _logicalDeviceDock)
                            {
                                info.DockInfo = _logicalDeviceDock.GetDockInfo();
                                try
                                {
                                    //IDevice iDevice = (IDevice)item;
                                    //if (iDevice != null)
                                    //{
                                    //    info.FirmwareVersion = iDevice.FirmwareVersion.ToString();
                                    //}
                                    info.DockPackageFwVersion = info.FirmwareVersion;
                                    byte[] dokc_bytes = _logicalDeviceDock.GetMonitorCount();
                                    _logs.DebugMsg_1($"[PeripheralsPlugin] GetMonitorCount byte is null = {(dokc_bytes == null ? "Yes" : "No")}");
                                    if (dokc_bytes != null)
                                    {
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] GetMonitorCount dokc_bytes.Length : {dokc_bytes.Length}");
                                        string textString = System.Text.Encoding.UTF8.GetString(dokc_bytes);
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] GetMonitorCount dokc_bytes to string : " + textString);
                                        if (!string.IsNullOrEmpty(textString))
                                        {
                                            try
                                            {
                                                using (JsonDocument doc = JsonDocument.Parse(textString))
                                                {
                                                    JsonElement root = doc.RootElement;
                                                    string payloadElement = root.GetProperty("Payload").ToString();
                                                    int temp_int = 0;
                                                    if (!string.IsNullOrEmpty(payloadElement) && int.TryParse(payloadElement, out temp_int))
                                                    {
                                                        info.MonitorCount = temp_int;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logs.DebugMsg_1($"[PeripheralsPlugin] GetMonitorCount Error : {ex.Message}");
                                            }
                                        }
                                    }
                                    dokc_bytes = _logicalDeviceDock.GetDockData();
                                    _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockData byte is null = {(dokc_bytes == null ? "Yes" : "No")}");
                                    if (dokc_bytes != null)
                                    {
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockData dokc_bytes.Length : {dokc_bytes.Length}");
                                        string textString = System.Text.Encoding.UTF8.GetString(dokc_bytes);
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockData dokc_bytes to string : " + textString);
                                        if (!string.IsNullOrEmpty(textString))
                                        {
                                            try
                                            {
                                                using (JsonDocument doc = JsonDocument.Parse(textString))
                                                {
                                                    JsonElement root = doc.RootElement;
                                                    JsonElement payloadElement = root.GetProperty("Payload");
                                                    DockData dockData = JsonSerializer.Deserialize<DockData>(payloadElement.GetRawText());
                                                    if (dockData != null)
                                                    {
                                                        info.DockData = dockData;
                                                        info.DockType = dockData.DockType;
                                                        info.ModelNumber = dockData.MarketingName;
                                                        //info.Name = $"Dell Dock";
                                                        if (info.ModelNumber.ToUpper().StartsWith("WD19S"))
                                                        {
                                                            info.ModelNumber = $"{dockData.MarketingName}_{dockData.PowerSupplyWattage}W";
                                                        }
                                                        if (string.IsNullOrEmpty(info.DockServiceTag) && !string.IsNullOrEmpty(dockData.ServiceTag))
                                                        {
                                                            info.DockServiceTag = dockData.ServiceTag;
                                                        }
                                                        //if (string.IsNullOrEmpty(info.DockPackageFwVersion) && !string.IsNullOrEmpty(dockData.PackageFirmwareVersion.ToString()))
                                                        //{
                                                        //    info.FirmwareVersion = dockData.PackageFirmwareVersion.ToString();
                                                        //    info.DockPackageFwVersion = dockData.PackageFirmwareVersion.ToString();
                                                        //}
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockData Error : {ex.Message}");
                                            }
                                        }
                                    }
                                    dokc_bytes = _logicalDeviceDock.GetDockFwUpdateStatus();
                                    _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockFwUpdateStatus byte is null = {(dokc_bytes == null ? "Yes" : "No")}");
                                    if (dokc_bytes != null)
                                    {
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockFwUpdateStatus dokc_bytes.Length : {dokc_bytes.Length}");
                                        string textString = System.Text.Encoding.UTF8.GetString(dokc_bytes);
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockFwUpdateStatus dokc_bytes to string : " + textString);
                                        if (!string.IsNullOrEmpty(textString))
                                        {
                                            try
                                            {
                                                using (JsonDocument doc = JsonDocument.Parse(textString))
                                                {
                                                    JsonElement root = doc.RootElement;
                                                    string payloadElement = root.GetProperty("Payload").ToString();
                                                    int temp_int = 0;
                                                    if (!string.IsNullOrEmpty(payloadElement) && int.TryParse(payloadElement, out temp_int))
                                                    {
                                                        info.DockFwUpdateStatus = temp_int;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockFwUpdateStatus Error : {ex.Message}");
                                            }
                                        }
                                    }
                                    dokc_bytes = _logicalDeviceDock.GetDockTBTConnectionStatus();
                                    _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockTBTConnectionStatus byte is null = {(dokc_bytes == null ? "Yes" : "No")}");
                                    if (dokc_bytes != null)
                                    {
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockTBTConnectionStatus dokc_bytes.Length : {dokc_bytes.Length}");
                                        string textString = System.Text.Encoding.UTF8.GetString(dokc_bytes);
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockTBTConnectionStatus dokc_bytes to string : " + textString);
                                        if (!string.IsNullOrEmpty(textString))
                                        {
                                            try
                                            {
                                                using (JsonDocument doc = JsonDocument.Parse(textString))
                                                {
                                                    JsonElement root = doc.RootElement;
                                                    string payloadElement = root.GetProperty("Payload").ToString();
                                                    int temp_int = 0;
                                                    if (!string.IsNullOrEmpty(payloadElement) && int.TryParse(payloadElement, out temp_int))
                                                    {
                                                        info.DockTBTConnectionStatus = temp_int;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockTBTConnectionStatus Error : {ex.Message}");
                                            }
                                        }
                                    }
                                    dokc_bytes = _logicalDeviceDock.GetDockServiceTag();
                                    _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockServiceTag byte is null = {(dokc_bytes == null ? "Yes" : "No")}");
                                    if (dokc_bytes != null)
                                    {
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockServiceTag dokc_bytes.Length : {dokc_bytes.Length}");
                                        string textString = System.Text.Encoding.UTF8.GetString(dokc_bytes);
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockServiceTag dokc_bytes to string : " + textString);
                                        if (!string.IsNullOrEmpty(textString))
                                        {
                                            try
                                            {
                                                using (JsonDocument doc = JsonDocument.Parse(textString))
                                                {
                                                    JsonElement root = doc.RootElement;
                                                    string payloadElement = root.GetProperty("Payload").ToString();
                                                    if (string.IsNullOrEmpty(info.DockServiceTag) && !string.IsNullOrEmpty(payloadElement))
                                                    {
                                                        info.DockServiceTag = payloadElement;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockServiceTag Error : {ex.Message}");
                                            }
                                        }
                                    }
                                    //dokc_bytes = _logicalDeviceDock.GetDockPackageFwVersion();
                                    //_logs.DebugMsg_1($"[PeripheralsPlugin] GetDockPackageFwVersion byte is null = {(dokc_bytes == null ? "Yes" : "No")}");
                                    //if (dokc_bytes != null)
                                    //{
                                    //    _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockPackageFwVersion dokc_bytes.Length : {dokc_bytes.Length}");
                                    //    string textString = System.Text.Encoding.UTF8.GetString(dokc_bytes);
                                    //    _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockPackageFwVersion dokc_bytes to string : " + textString);
                                    //    if (!string.IsNullOrEmpty(textString))
                                    //    {
                                    //        try
                                    //        {
                                    //            using (JsonDocument doc = JsonDocument.Parse(textString))
                                    //            {
                                    //                JsonElement root = doc.RootElement;
                                    //                string payloadElement = root.GetProperty("Payload").ToString();
                                    //                if (string.IsNullOrEmpty(info.DockPackageFwVersion) && !string.IsNullOrEmpty(payloadElement))
                                    //                {
                                    //                    info.DockPackageFwVersion = payloadElement;
                                    //                    info.FirmwareVersion = payloadElement;
                                    //                }
                                    //            }
                                    //        }
                                    //        catch (Exception ex)
                                    //        {
                                    //            _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockPackageFwVersion Error : {ex.Message}");
                                    //        }
                                    //    }
                                    //}
                                }
                                catch (Exception ex)
                                {
                                    _logs.DebugMsg_1($"[PeripheralsPlugin] Dock Data Error : {ex.Message}");
                                }
                            }
                            _deviceHelper.deviceInfo.Add(info);

                            //item.update
                        }
                        //_iDeviceManager_DeviceAddedEvent(device);
                    }
                }
                Console.WriteLine(_deviceHelper.ToString());
                writelog(_deviceHelper.ToString());
            }
        }

        private void Pen_KeyCaptureProgressDataChanged(ILogicalDevicePen arg1, string arg2)
        {
            Debug.WriteLine($"Pen: {arg1.Id} KeyCaptureProgressDataChangedString, newValue: {arg2}");
            writelog($"Pen: {arg1.Id} KeyCaptureProgressDataChanged, newValue: {arg2}");
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = $"PenKeyCaptureProgressDataChanged|{arg2}";
                OnNotify(_EventArgs);
            }
        }

        private void Pen_KeyCaptureDataChanged(ILogicalDevicePen arg1, string arg2)
        {
            Debug.WriteLine($"Pen: {arg1.Id} KeyCaptureDataChangedString, newValue: {arg2}");
            writelog($"Pen: {arg1.Id} KeyCaptureDataChanged, newValue: {arg2}");
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "PenKeyCaptureDataChanged";
                OnNotify(_EventArgs);
            }
        }

        private void Pen_KeyCaptureStarted(ILogicalDevicePen obj)
        {
            Debug.WriteLine($"Pen: {obj.Id} KeyCaptureStarted!");
            writelog($"Pen: {obj.Id} KeyCaptureStarted!");
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == obj.Id.ToString());
                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "PenKeyCaptureStarted";
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalDevice_PairedHostNameChanged(ILogicalDevice3 arg1, int arg2, string arg3)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo != null)
                {
                    Debug.WriteLine($"Guid:{deviceInfo.ID} PairedHostNameChanged {arg2}:{arg3}");
                    writelog($"Guid:{deviceInfo.ID} PairedHostNameChanged {arg2}:{arg3}");
                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "PairedHostNameChanged";
                    OnNotify(_EventArgs);
                }
            }
        }

        private void Pen_PenSettingChanged(ILogicalDevicePen arg1, string arg2)
        {
            Debug.WriteLine(arg2);
            writelog(arg2);
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
            else if (device is IPhysicalDeviceDongle _iPhysicalDeviceDongle)
            {
                DongleInfo rfInfo = new DongleInfo();
                var rfdongle = _rfDeviceHelper.dongleInfo.Where(x => x.DeviceType == _iPhysicalDeviceDongle.Type).FirstOrDefault();
                if (rfdongle != null)
                {
                    rfdongle.IsMultipleDongleFound = true;
                }
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
                lock (_lock)
                {
                    _isClientConnected = true;

                    _iClient = client;
                    _iDeviceManager = _iClient.DeviceManager;
                    _iDeviceManager.DeviceAddedEvent += _iDeviceManager_DeviceAddedEvent;
                    _iDeviceManager.DeviceRemovedEvent += _iDeviceManager_DeviceRemovedEvent;
                    // First go through existing PhysicalDevices and add events
                    foreach (var iPhysicalDevice in _iDeviceManager.Devices)
                    {
                        iPhysicalDevice.DeviceAddedEvent += IPhysicalDevice_DeviceAddedEvent;
                        iPhysicalDevice.DeviceRemovedEvent += IPhysicalDevice_DeviceRemovedEvent;

                    }

                    _iUpdateManager = _iClient.UpdateManager;
                    _iUpdateManager.IsAnyUpdateAvailableChanged += IUpdateManager_IsAnyUpdateAvailableChanged;


                    _iOverlayManager = _iClient.OverlayManager;
                    _iOverlayManager.VolatileSettingsChanged += _iOverlayManager_VolatileSettingsChanged;

                    _iCTKMessageHelper = _iClient.CTKMessageHelper;
                    _iCTKMessageHelper.CollaborationMsgChanged += _iCTKMessageHelper_CollaborationMsgChanged;
                    _iCTKMessageHelper.CollabMultipleCallsDetectedChanged += _iCTKMessageHelper_CollabMultipleCallsDetectedChanged;
                    _iCTKMessageHelper.IsZoomMultipleCallsDetectedChanged += _iCTKMessageHelper_IsZoomMultipleCallsDetectedChanged;
                    _iCTKMessageHelper.IsZoomCallbacksRegisteredChanged += _iCTKMessageHelper_IsZoomCallbacksRegisteredChanged;

                    // Check for any updates
                    IUpdateManager_IsAnyUpdateAvailableChanged(_iUpdateManager.IsAnyUpdateAvailable);

                    ScanDevices();
                }

            }
            else
            {
                lock (_lock)
                {
                    if (_isClientConnected)
                    {
                        _isClientConnected = false;
                        _iDeviceManager = null;
                        _iUpdateManager = null;
                        _iClient = null;

                        _deviceHelper = new DeviceHelper
                        {
                            deviceInfo = new List<DeviceInfo>()
                        };

                        UpdateAvailable = false;

                        _updateHelper = new UpdateHelper
                        {
                            UpdateItems = new List<UpdateItemInfo>()
                        };

                        //OnNotify(EventArgs.Empty);
                    }

                }


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
            Debug.WriteLine(_clientInfo.ToString());
            writelog(_clientInfo.ToString());
        }

        private void _iCTKMessageHelper_IsZoomCallbacksRegisteredChanged(bool obj)
        {
            IsZoomCallbacksRegisteredChanged?.Invoke(this, obj);
        }

        private void _iCTKMessageHelper_IsZoomMultipleCallsDetectedChanged(bool obj)
        {
            IsZoomMultipleCallsDetectedChanged?.Invoke(this, obj);
        }

        private void _iCTKMessageHelper_CollabMultipleCallsDetectedChanged(bool obj)
        {
            CollabMultipleCallsDetectedChanged?.Invoke(this, obj);
            Debug.WriteLine($"{obj}");
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
            if (PhysicalDevices.Contains(iPhysicalDevice.Id))
            { return; }

            System.Diagnostics.Debug.WriteLine("ParentPhysicalDevice Added, Id : " + iPhysicalDevice.Id + ", Name : " + iPhysicalDevice.Name);

            lock (_lock)
            {
                if (_isClientConnected)
                {
                    iPhysicalDevice.DeviceAddedEvent += IPhysicalDevice_DeviceAddedEvent;
                    iPhysicalDevice.DeviceRemovedEvent += IPhysicalDevice_DeviceRemovedEvent;
                    PhysicalDevices.Add(iPhysicalDevice.Id);
                    ScanDevices();
                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_PlugIn;
                    _EventArgs.changedProperty = "PhysicalDeviceAdded";
                    OnNotify(_EventArgs);
                }
            }
        }

        private void _iDeviceManager_DeviceRemovedEvent(IPhysicalDevice iPhysicalDevice)
        {
            System.Diagnostics.Debug.WriteLine("ParentPhysicalDevice Removed, Id : " + iPhysicalDevice.Id + ", Name : " + iPhysicalDevice.Name);
            lock (_PeripheralLock)
            {
                if (_isClientConnected)
                {
                    iPhysicalDevice.DeviceAddedEvent -= IPhysicalDevice_DeviceAddedEvent;
                    iPhysicalDevice.DeviceRemovedEvent -= IPhysicalDevice_DeviceRemovedEvent;

                    ScanDevices();

                    if (PhysicalDevices1.Contains(iPhysicalDevice.Id))
                    {
                        PhysicalDevices1.Remove(iPhysicalDevice.Id);

                    }
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
            lock (_lock)
            {
                if (_isClientConnected)
                {
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
            }
        }

        private void IPhysicalDevice_DeviceRemovedEvent(ILogicalDevice iLogicalDevice)
        {
            lock (_lock)
            {

                if (_isClientConnected)
                {
                    Debug.WriteLine($"ID: {iLogicalDevice.Id}, Type:{iLogicalDevice.Type}");
                    Debug.WriteLine($"DeviceCount: {_deviceHelper.deviceInfo.Count}");
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
                    ScanDevices();
                }
            }
        }

        private void ILogicalDevice_DpiLevelChanged(ILogicalDevice2 arg1, int arg2)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Debug.WriteLine(arg2.ToString());
                writelog(arg2.ToString());
                if (deviceInfo == null)
                    return;
                if (arg2 == 0)
                    return;
                //deviceInfo.DpiLevel = arg2 - 1;
                deviceInfo.DpiLevel = arg2;
                Debug.WriteLine($"New DpiLevel: {arg2}");

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "DpiLevelChanged";
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalDevice_DpiValueChanged(ILogicalDevice3 arg1, int arg2)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Debug.WriteLine(arg2.ToString());
                writelog(arg2.ToString());
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
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Debug.WriteLine(arg2.ToString());
                writelog(arg2.ToString());
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
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Debug.WriteLine(arg2.ToString());
                writelog(arg2.ToString());
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
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Debug.WriteLine(arg2.ToString());
                writelog(arg2.ToString());
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
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                Debug.WriteLine(arg2.ToString());
                writelog(arg2.ToString());
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
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.BatteryLevel = arg2;

                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "BatteryLevelChanged";
                    OnNotify(_EventArgs);

                    if (arg2 >= 0 && arg2 <= 9)
                    {
                        OSDType_Device type = OSDType_Device.Unknown;
                        var deviceType = deviceInfo.LogicalDeviceType.ToUpper();
                        if (deviceType.Contains("PEN"))
                        {
                            if (deviceInfo.ModelNumber == "PN5122W" && arg2 > 6)
                            { return; }
                            type = OSDType_Device.Pen;
                        }
                        else if (deviceType.Contains("KEYBOARD"))
                        {
                            type = OSDType_Device.Keyboard;
                        }
                        else if (deviceType.Contains("MOUSE"))
                        {
                            type = OSDType_Device.Mouse;
                        }
                        else if (deviceType.Contains("HEADSET"))
                        {
                            type = OSDType_Device.Headset;
                        }
                        _ = _DeviceManagerPlugin.ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.BatteryLow, type, deviceInfo.Name);
                    }
                }
            }
        }

        private void ILogicalWiredAudio_MuteStatusChanged(ILogicalWiredAudio arg1, bool newMuteStatus)
        {
            Console.WriteLine(newMuteStatus.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] ILogicalWiredAudio_MuteStatusChanged ... in " + newMuteStatus.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] ILogicalWiredAudio_MuteStatusChanged ... out " + newMuteStatus.ToString() + " , OSD in");
                if (_DeviceManagerPlugin.GetGlobalSettingParam().Result.GlobalSetting_General.Display_MuteState)
                {
                    Task.Run(async () => _DeviceManagerPlugin.ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.Mute, deviceInfo.Name, newMuteStatus));
                }
                _logs.DebugMsg_1("[PeripheralsPlugin] ILogicalWiredAudio_MuteStatusChanged ... OSD out ");
            }
        }

        private void _logicalDeviceHeadset_IsReadyChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_IsReadyChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_IsReadyChanged ... out " + newValue.ToString() + ", " + deviceInfo.FirmwareVersion);
            }
        }

        private void _logicalDeviceHeadset_IsDirtyChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_IsDirtyChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_IsDirtyChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_MicNoiseCancellationChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MicNoiseCancellationChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MicNoiseCancellationChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_MicNCIncomingChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MicNCIncomingChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MicNCIncomingChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_BusyLightChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_BusyLightChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_BusyLightChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_VoiceGuidanceChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_VoiceGuidanceChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_VoiceGuidanceChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_MuteStatusChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MuteStatusChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MuteStatusChanged ... out " + newValue.ToString() + " , OSD in ...");
                if (_DeviceManagerPlugin.GetGlobalSettingParam().Result.GlobalSetting_General.Display_MuteState)
                {
                    Task.Run(async () => _ = _DeviceManagerPlugin.ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.Mute, deviceInfo.Name, newValue));
                }
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MuteStatusChanged OSD ... out ");
            }
        }

        private void _logicalDeviceHeadset_SidetoneChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_SidetoneChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_SidetoneChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_SelectedPresetChanged(ILogicalDeviceHeadset logicalDeviceHeadset, int newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_SelectedPresetChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_SelectedPresetChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_SidetoneLevelChanged(ILogicalDeviceHeadset logicalDeviceHeadset, int newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_SidetoneLevelChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_SidetoneLevelChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_BandsGainChanged(ILogicalDeviceHeadset logicalDeviceHeadset, byte[] newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_BandsGainChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_BandsGainChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_AncModeChanged(ILogicalDeviceHeadset logicalDeviceHeadset, int newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_AncModeChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_AncModeChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_AncGainChanged(ILogicalDeviceHeadset logicalDeviceHeadset, int newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_AncGainChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_AncGainChanged ... out " + newValue.ToString());
            }
        }

        private void _logicalDeviceHeadset_WearDetectionChanged(ILogicalDeviceHeadset logicalDeviceHeadset, int newValue)
        {
            Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_WearDetectionChanged ... in " + newValue.ToString());
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
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_WearDetectionChanged ... out " + newValue.ToString());
            }
        }

        private void IUpdateManager_IsAnyUpdateAvailableChanged(bool isAnyUpdateAvailable)
        {
            lock (_lock)
            {
                if (_isClientConnected && _iUpdateManager != null)
                {
                    _logs.DebugMsg_1($"[PeripheralsPlugin] IUpdateManager_IsAnyUpdateAvailableChanged start");
                    _updateHelper = new UpdateHelper();
                    UpdateAvailable = isAnyUpdateAvailable;
                    _updateHelper.UpdateItems = new List<UpdateItemInfo>();
                    if (_iUpdateManager != null && _iUpdateManager.AllUpdateItems != null)
                    {
                        _logs.DebugMsg_1($"[PeripheralsPlugin] _iUpdateManager.AllUpdateItems.Count = {_iUpdateManager.AllUpdateItems.Count}");
                        foreach (var updateItem in _iUpdateManager.AllUpdateItems)
                        {
                            _updateItems = new UpdateItemInfo() { UpdateType = updateItem.Type.ToString(), UpdateSeverity = updateItem.Severity.ToString(), NewVersion = updateItem.NewVersion, Description = updateItem.Description };

                            _updateItems.CurrentVersion = updateItem.CurrentVersion;
                            _updateItems.DeviceId = updateItem.DeviceId;
                            _updateItems.DeviceIndex = updateItem.DeviceIndex;
                            _updateItems.DeviceModelNumber = updateItem.DeviceModelNumber;
                            _logs.DebugMsg_1($"[PeripheralsPlugin] _updateItems.DeviceModelNumber = {_updateItems.DeviceModelNumber}");
                            _updateItems.DeviceName = updateItem.DeviceName;
                            _logs.DebugMsg_1($"[PeripheralsPlugin] _updateItems.DeviceName = {_updateItems.DeviceName}");
                            _updateItems.DevicePath = updateItem.DevicePath;
                            _updateItems.DeviceType = updateItem.DeviceType;
                            _logs.DebugMsg_1($"[PeripheralsPlugin] _updateItems.DeviceType = {_updateItems.DeviceType}");
                            _updateItems.FrimwareUpdatePath = updateItem.FrimwareUpdatePath;
                            _updateItems.InstallPath = updateItem.InstallPath;
                            _updateItems.InstanceId = updateItem.InstanceId;
                            _updateItems.Priority = updateItem.Priority;
                            _updateItems.ServerPath = updateItem.ServerPath;
                            _updateItems.SupplierID = updateItem.SupplierID;
                            _updateItems.SHA256 = updateItem.SHA256;
                            //_updateItems.SHA512 = updateItem.SHA512;
                            _updateItems.Thumbprint = updateItem.Thumbprint;

                            _updateHelper.UpdateItems.Add(_updateItems);
                        }
                    }
                    else
                    {
                        _logs.DebugMsg_1($"[PeripheralsPlugin] _iUpdateManager.AllUpdateItems is null");
                    }

                    Console.WriteLine(isAnyUpdateAvailable ? "UpdateAvailable" : "Already Updated.");
                    _logs.DebugMsg_1($"[PeripheralsPlugin] isAnyUpdateAvailable = {(isAnyUpdateAvailable ? "UpdateAvailable" : "Already Updated.")}");
                }
                if (isAnyUpdateAvailable)
                {
                    OnUpdateNotify(isAnyUpdateAvailable);
                }
                _logs.DebugMsg_1($"[PeripheralsPlugin] IUpdateManager_IsAnyUpdateAvailableChanged done");
            }
        }

        private void IPhysicalDeviceDongle_PairedDeviceCountChanged(IPhysicalDeviceDongle physicalDeviceDongle, int newValue)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == physicalDeviceDongle.Id.ToString());
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
                    Debug.WriteLine($"{deviceInfo.PairingStatusName}");
                    OnNotify(_EventArgs);
                }
            }
        }

        private void PhysicalAudioDeviceDongle_PairingStatusChanged(IPhysicalAudioDeviceDongle Arg1, int nArg2, int nArg3, string strArg4)//(IPhysicalAudioDeviceDongle arg1, AudioDonglePairingStatus arg2)
        {
            throw new NotImplementedException();
        }

        private void PhysicalAudioDeviceDongle_PairedDeviceCountChanged(IPhysicalAudioDeviceDongle arg1, int arg2)
        {
            //var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == arg1.Id.ToString());

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
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == iLogicalDeviceWebcam.Id.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.IsMicEnumerationOn = newValue;

                    DeviceChangedEventArgs _EventArgs = new()
                    {
                        type = DeviceChangedType.Peripherals_SettingsChange,
                        device_peripherals = deviceInfo,
                        changedProperty = "IsMicEnumerationOn"
                    };
                    OnNotify(_EventArgs);
                }
            }
        }

        private void ILogicalDevice_MousePrimaryButtonChanged(ILogicalDevice3 logicalDevice3, MouseButton newValue)
        {
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
            _logs.DebugMsg_1($"[PeripheralsPlugin] GetFWUpdateInfo start");
            if (_isClientConnected && _updateHelper != null)
            {
                _logs.DebugMsg_1($"[PeripheralsPlugin] GetFWUpdateInfo, _updateHelper.UpdateItems = {_updateHelper.UpdateItems.Count}");
                _logs.DebugMsg_1($"[PeripheralsPlugin] GetFWUpdateInfo done and _updateHelper is no null");
                return await Task.Run(() =>
                {
                    lock (_lock)
                    {
                        if (_isClientConnected && _updateHelper != null)
                        {
                            return _updateHelper;

                        }
                        else
                        {
                            return new UpdateHelper()
                            {
                                UpdateItems = new List<UpdateItemInfo>()
                            };
                        }

                    }
                });
            }
            _logs.DebugMsg_1($"[PeripheralsPlugin] GetFWUpdateInfo done but _updateHelper is null");
            return new UpdateHelper()
            {
                UpdateItems = new List<UpdateItemInfo>()
            };
        }
        //Bruce, FWU need it
        public int GetDongleCount()
        {
            return _DongleCount;
        }

        public void DisplayNotification(string bannerInfo, string hyperlinkText, string bannerItemType)
        {
        }

        public void CheckForUpdate()
        {
            if (_isClientConnected && _iUpdateManager != null)
            {
                _iUpdateManager.CheckForUpdate();
            }
        }

        #endregion
        private void writelog(string text,
                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
                log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[PeripheralsPlugin] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
            Console.WriteLine(text);
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        private enum log_type
        {
            info = 0,
            error
        }


        private void InitializeDeviceManagerPlugin()
        {
            if (_DeviceManagerPlugin != null)
                return;

            _DeviceManagerPlugin = _agent.PluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

            if (_DeviceManagerPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnDeviceManagerPluginConditionChangeHandler;
                GetCurrentDeviceManagerPluginCondition();
            }
        }
        private void OnDeviceManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDeviceManagerPluginCondition();
        }
        private void GetCurrentDeviceManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DeviceManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                lock (_PluginConditionLock_DeviceManager)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentDeviceManagerPluginCondition)} - DeviceManager Plugin is in an error condition");
                        //_PeripheralsPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentDeviceManagerPluginCondition)} - DeviceManager Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentDeviceManagerPluginCondition)} - DeviceManager Plugin is in a started condition");
                    }
                }
            });
        }
    }
}