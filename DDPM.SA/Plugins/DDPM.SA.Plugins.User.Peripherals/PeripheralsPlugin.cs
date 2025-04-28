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
using DDPM.SA.Common.Settings;
using DDPM.SA.Common.UI;
using DDPM.SA.Resources.Helper;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using DPeMPublic.Common.Enums;
using IndiLogic.DPeM.Broker;
using Microsoft;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using VcpCore.Common;
using IDeviceManager = IndiLogic.DPeM.Broker.IDeviceManager;
using IDs = DDPM.SA.Common.IDs;
using Task = System.Threading.Tasks.Task;

namespace DDPM.SA.Plugins.PeripheralsPlugin
{
    [Plugin(IDs.DDPM_PERIPHERALS_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IDPeMPlugin) })]
    public class PeripheralsPlugin : BaseAgentPlugin, IDPeMPlugin, IDisposableObservable
    {
        private object _PeripheralLock = new object();

        #region Properties and fields

        private const string pluginName = "PeripheralsPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements DDPM Peripherals Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements DDPM Peripherals Plugin.";

        private IAgent _agent;
        public const string PluginLogId = "Peripherals";

        private DeviceHelper _deviceHelper;
        private UpdateHelper _updateHelper;
        //Bruce, FWU need it
        private int _IODongleCount_Gen3Ago;
        private int _DockCount;
        private RFDeviceHelper _rfDeviceHelper;
        private ClientInfo _clientInfo;
        private static Logs _logs;
        private bool _IsConnectingMultipleDocks = false;
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
        private static List<Guid> PhysicalPenDevices = new();
        private static List<Guid> PhysicalDevices2 = new();
        private static List<Guid> LogicalDevices = new();
        private static List<Guid> LogicalDevices2 = new();
        private static List<Guid> LogicalDevices3 = new();
        private static List<Guid> LogicalDevicesPen = new();
        private static List<Guid> LogicalDevicHeadset = new();
        private static List<Guid> LogicalWiredAudio = new();
        private static List<Guid> IDevices = new();

        //private IDeviceManagerSA _DeviceManagerPlugin;
        //private readonly object _PluginConditionLock_DeviceManager = new object();
        //private readonly object _lock = new();

        private IDTPProxyPlugin _DTPProxyPlugin = null;
        private ISettingsManagerDev _UserSettingsPlugin = null;

        private readonly object _Lock = new object();
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
                return await System.Threading.Tasks.Task.Run(() =>
                {
                    lock (_Lock)//this)
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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

        //public void SetPrimaryMouseButton(MouseButton newMouseButton, Guid deviceId)
        //{
        //    foreach (var device in _iDeviceManager.Devices)
        //    {
        //        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        //        if (logicalDevice is ILogicalDevice3 _logicalDevice3)
        //        {
        //            DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
        //            if (_deviceInfo != null && _deviceInfo.MousePrimaryButton != newMouseButton)
        //            {
        //                _logicalDevice3.SetPrimaryMouseButton(newMouseButton);
        //                _deviceInfo.MousePrimaryButton = newMouseButton;
        //                break;
        //            }
        //        }
        //    }
        //}

        public void SetTouchScrollSensitivityLevel(int newTouchScrollSensitivityLevel, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDevice3 _logicalDevice3)
                {
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                    if (_deviceInfo != null && _deviceInfo.BackLightingLevel != newValue)
                    {
                        _logicalDevice3.SetBackLightingLevel(newValue);
                        _deviceInfo.BackLightingLevel = newValue;
                        writelog($"SetBackLightingLevel: value:{newValue}.....................{DateTime.Now:HH:mm:ss.ff}");
                        break;
                    }
                }
            }
        }

        public void StartPairing(Guid physicalDeviceId)
        {
            if (physicalDeviceId != Guid.Empty)
            {
                writelog($"StartPairing : " + physicalDeviceId.ToString());
            }
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
                    writelog($"StartPairing: AudioDeviceDongle : physicalDevice.Name = {physicalDevice.Name}; physicalDeviceId = " + physicalDeviceId.ToString());
                }
            }
        }

        public void StopPairing(Guid physicalDeviceId)
        {
            if (physicalDeviceId != Guid.Empty)
            {
                writelog($"StopPairing : " + physicalDeviceId.ToString());
            }
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
                    writelog($"StopPairing: AudioDeviceDongle : physicalDevice.Name = {physicalDevice.Name}; physicalDeviceId = " + physicalDeviceId.ToString());
                }
            }
        }

        public void UnPair(Guid logicalDeviceId)
        {
            if (logicalDeviceId != Guid.Empty)
            {
                writelog($"UnPair : " + logicalDeviceId.ToString());
            }
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
                        writelog($"UnPair: IPhysicalAudioDeviceDongle : logicalDevice.Name = {logicalDevice.Name.ToString()}; logicalDeviceId = " + logicalDeviceId.ToString());
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
                    //Change to DTP
                    //_DeviceManagerPlugin.SetIsWiredAudioIMicNSEnableAsync(deviceId.ToString(), newValue);
                    try
                    {
                        _DTPProxyPlugin.SetIsWiredAudioIMicNSEnableAsync(deviceId.ToString(), newValue);
                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.IsWiredAudioIMicNSEnable = newValue;
                            writelog($"DTP:{nameof(SetWiredAudioIMicNSEnable)} device({device.Name}) IsWiredAudioIMicNSEnable:{newValue}");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetWiredAudioIMicNSEnable)} device({device.Name}) exception with ({e.Message})");
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
                    //Change to DTP
                    //_DeviceManagerPlugin.SetIsWiredAudioMicMuteSoundEnableAsync(deviceId.ToString(), newValue);
                    try
                    {
                        _DTPProxyPlugin.SetIsWiredAudioMicMuteSoundEnableAsync(deviceId.ToString(), newValue);
                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.IsWiredAudioMicMuteSoundEnable = newValue;
                            writelog($"DTP:{nameof(SetWiredAudioMicMuteSoundEnable)} device({device.Name}) IsWiredAudioMicMuteSoundEnable:{newValue}");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetWiredAudioMicMuteSoundEnable)} device({device.Name}) exception with ({e.Message})");
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
                    //Change to DTP
                    //_DeviceManagerPlugin.SetWiredAudioVolumeAdjustmentToneAsync(deviceId.ToString(), newValue);
                    try
                    {
                        _DTPProxyPlugin.SetWiredAudioVolumeAdjustmentToneAsync(deviceId.ToString(), newValue);
                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.WiredAudioVolumeAdjustmentTone = newValue;
                            writelog($"DTP:{nameof(SetWiredAudioVolumeAdjustmentTone)} device({device.Name}) WiredAudioVolumeAdjustmentTone:{newValue}");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetWiredAudioVolumeAdjustmentTone)} device({device.Name}) exception with ({e.Message})");
                    }
                }
            }
        }

        public void SetWiredAudioPreset(int newValue, Guid deviceId, string bandGainNumber)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalWiredAudio _logicalWiredAudioDevice)
                {
                    try
                    {
                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            switch (bandGainNumber)
                            {
                                case "Bass":
                                    //_DeviceManagerPlugin.SetBassAsync(deviceId.ToString(), newValue);
                                    _DTPProxyPlugin.SetBassAsync(deviceId.ToString(), newValue);
                                    break;
                                case "Mid":
                                    //_DeviceManagerPlugin.SetMidRangeAsync(deviceId.ToString(), newValue);
                                    _DTPProxyPlugin.SetMidRangeAsync(deviceId.ToString(), newValue);
                                    break;
                                case "Treble":
                                    //_DeviceManagerPlugin.SetTrebleAsync(deviceId.ToString(), newValue);
                                    _DTPProxyPlugin.SetTrebleAsync(deviceId.ToString(), newValue);
                                    break;
                            }
                            writelog($"DTP:{nameof(SetWiredAudioPreset)} device({device.Name}) {bandGainNumber}:{newValue}");
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetWiredAudioPreset)} device({device.Name}) exception with ({e.Message})");
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
                    try
                    {
                        //if (!_DeviceManagerPlugin.SetSidetoneLevelAsync(deviceId.ToString(), newValue).Result)// DTP
                        if (!_DTPProxyPlugin.SetSidetoneLevelAsync(deviceId.ToString(), newValue).Result)
                        {
                            _logicalDeviceHeadset.SetSidetoneLevel(newValue); //DTH
                            writelog($"DTH:{nameof(SetSidetoneLevel)} device({device.Name}) SidetoneLevel:{newValue}");
                        }
                        else
                            writelog($"DTP:{nameof(SetSidetoneLevel)} device({device.Name}) SidetoneLevel:{newValue}");

                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.SidetoneLevel = newValue;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetSidetoneLevel)} device({device.Name}) exception with ({e.Message})");
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
                    try
                    {
                        //if (!_DeviceManagerPlugin.SetAncModeAsync(deviceId.ToString(), newValue).Result)
                        if (!_DTPProxyPlugin.SetAncModeAsync(deviceId.ToString(), newValue).Result)
                        {
                            _logicalDeviceHeadset.SetAncMode(newValue);
                            writelog($"DTH:{nameof(SetAncMode)} device({device.Name}) SetAncModeAsync:{newValue}");
                        }
                        else
                            writelog($"DTP:{nameof(SetAncMode)} device({device.Name}) SetAncModeAsync:{newValue}");

                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.AncMode = newValue;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetAncMode)} device({device.Name}) exception with ({e.Message})");
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
                    try
                    {
                        //if (!_DeviceManagerPlugin.SetAncGainAsync(deviceId.ToString(), newValue).Result)
                        if (!_DTPProxyPlugin.SetAncGainAsync(deviceId.ToString(), newValue).Result)
                        {
                            _logicalDeviceHeadset.SetAncGain(newValue);
                            writelog($"DTH:{nameof(SetAncGain)} device({device.Name}) SetAncGainAsync:{newValue}");
                        }
                        else
                            writelog($"DTP:{nameof(SetAncGain)} device({device.Name}) SetAncGainAsync:{newValue}");

                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.AncGain = newValue;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetAncGain)} device({device.Name}) exception with ({e.Message})");
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
                    try
                    {
                        //if (!_DeviceManagerPlugin.SetSelectedPresetAsync(deviceId.ToString(), newValue).Result)
                        if (!_DTPProxyPlugin.SetSelectedPresetAsync(deviceId.ToString(), newValue).Result)
                        {
                            _logicalDeviceHeadset.SetSelectedPreset(newValue);
                            writelog($"DTH:{nameof(SetSelectedPreset)} device({device.Name}) SetSelectedPresetAsync:{newValue}");
                        }
                        else
                            writelog($"DTP:{nameof(SetSelectedPreset)} device({device.Name}) SetSelectedPresetAsync:{newValue}");

                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.SelectedPreset = newValue;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetSelectedPreset)} device({device.Name}) exception with ({e.Message})");
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                    if (_deviceInfo != null)
                    {
                        bool result = true;
                        try
                        {
                            switch (bandGainNumber)
                            {
                                case "band1gain":
                                    //if (!_DeviceManagerPlugin.SetBand1GainAsync(deviceId.ToString(), newValue).Result)
                                    if (!_DTPProxyPlugin.SetBand1GainAsync(deviceId.ToString(), newValue).Result)
                                    {
                                        _deviceInfo.Band1Gain = newValue;
                                        result = false;
                                    }
                                    break;
                                case "band2gain":
                                    //if (!_DeviceManagerPlugin.SetBand2GainAsync(deviceId.ToString(), newValue).Result)
                                    if (!_DTPProxyPlugin.SetBand2GainAsync(deviceId.ToString(), newValue).Result)
                                    {
                                        _deviceInfo.Band2Gain = newValue;
                                        result = false;
                                    }
                                    break;
                                case "band3gain":
                                    //if (!_DeviceManagerPlugin.SetBand3GainAsync(deviceId.ToString(), newValue).Result)
                                    if (!_DTPProxyPlugin.SetBand3GainAsync(deviceId.ToString(), newValue).Result)
                                    {
                                        _deviceInfo.Band3Gain = newValue;
                                        result = false;
                                    }
                                    break;
                                case "band4gain":
                                    //if (!_DeviceManagerPlugin.SetBand4GainAsync(deviceId.ToString(), newValue).Result)
                                    if (!_DTPProxyPlugin.SetBand4GainAsync(deviceId.ToString(), newValue).Result)
                                    {
                                        _deviceInfo.Band4Gain = newValue;
                                        result = false;
                                    }
                                    break;
                                case "band5gain":
                                    //if (!_DeviceManagerPlugin.SetBand5GainAsync(deviceId.ToString(), newValue).Result)
                                    if (!_DTPProxyPlugin.SetBand5GainAsync(deviceId.ToString(), newValue).Result)
                                    {
                                        _deviceInfo.Band5Gain = newValue;
                                        result = false;
                                    }
                                    break;

                            }
                            writelog($"DTP:{nameof(SetBandsGain)} device({device.Name}) {bandGainNumber}:{newValue}");
                        }
                        catch (Exception e)
                        {
                            writelog($"DTP:{nameof(SetBandsGain)} device({device.Name}) exception with ({e.Message})");
                            result = false;
                        }
                        if (!result)
                        {
                            _logicalDeviceHeadset.SetBandsGain(_deviceInfo.Band1Gain, _deviceInfo.Band2Gain,
                               _deviceInfo.Band3Gain, _deviceInfo.Band4Gain, _deviceInfo.Band5Gain);
                            writelog($"DTH:{nameof(SetBandsGain)} device({device.Name}) SetBandsGain");
                        }
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
                    try
                    {
                        //if (!_DeviceManagerPlugin.SetMicNoiseCancellationAsync(deviceId.ToString(), newValue).Result)
                        if (!_DTPProxyPlugin.SetMicNoiseCancellationAsync(deviceId.ToString(), newValue).Result)
                        {
                            _logicalDeviceHeadset.SetMicNoiseCancellation(newValue);
                            writelog($"DTH:{nameof(SetMicNoiseCancellation)} device({device.Name}) SetMicNoiseCancellationAsync:{newValue}");
                        }
                        else
                            writelog($"DTP:{nameof(SetMicNoiseCancellation)} device({device.Name}) SetMicNoiseCancellationAsync:{newValue}");

                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.MicNoiseCancellation = newValue;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetMicNoiseCancellation)} device({device.Name}) exception with ({e.Message})");
                    }
                }
            }
        }

        public void SetMicNoiseCancellationForMito(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    try
                    {
                        // Only have DTP
                        //_DeviceManagerPlugin.SetMicNoiseCancellationAsync(deviceId.ToString(), newValue);
                        //_DeviceManagerPlugin.SetMicNCIncomingAsync(deviceId.ToString(), newValue);
                        _DTPProxyPlugin.SetMicNoiseCancellationAsync(deviceId.ToString(), newValue);
                        _DTPProxyPlugin.SetMicNCIncomingAsync(deviceId.ToString(), newValue);
                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.MicNoiseCancellation = newValue;
                            writelog($"DTP:{nameof(SetMicNoiseCancellationForMito)} device({device.Name}) SetMicNoiseCancellationForMito:{newValue}");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetMicNoiseCancellationForMito)} device({device.Name}) exception with ({e.Message})");
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
                    try
                    {
                        //if (!_DeviceManagerPlugin.SetSidetoneAsync(deviceId.ToString(), newValue).Result)
                        if (!_DTPProxyPlugin.SetSidetoneAsync(deviceId.ToString(), newValue).Result)
                        {
                            _logicalDeviceHeadset.SetSidetone(newValue);
                            writelog($"DTH:{nameof(SetSidetone)} device({device.Name}) SetSidetone:{newValue}");
                        }
                        else
                            writelog($"DTP:{nameof(SetSidetone)} device({device.Name}) SetSidetoneAsync:{newValue}");

                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.Sidetone = newValue;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetSidetone)} device({device.Name}) exception with ({e.Message})");
                    }
                }
            }
        }

        public void SetWearDetection(int newValue, Guid deviceId)
        {
            //Elie. R17.1 drop this function. 1123
            //Wayn change to DTP
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    try
                    {
                        // Only have DTP
                        bool setvalue = (newValue == 1 ? true : false);
                        //_DeviceManagerPlugin.SetWearDetectionAsync(deviceId.ToString(), setvalue);
                        _DTPProxyPlugin.SetWearDetectionAsync(deviceId.ToString(), setvalue);
                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.WearDetection = newValue;
                            writelog($"DTP:{nameof(SetWearDetection)} device({device.Name}) SetWearDetection:{newValue}");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetWearDetection)} device({device.Name}) exception with ({e.Message})");
                    }
                }
            }
        }
        public void SetWearDetectionForCLI(int newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                //Wayn R17.2 Change to DTP 12/2
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    try
                    {
                        bool setvalue = (newValue == 1 ? true : false);
                        if (setvalue) // If WearDetection need to turn On
                        {
                            //_DeviceManagerPlugin.SetWearDetectionAsync(deviceId.ToString(), setvalue); // Set WearDetection On
                            _DTPProxyPlugin.SetWearDetectionAsync(deviceId.ToString(), setvalue);
                            //if (!_DeviceManagerPlugin.GetIsWearDetectionMuteMicEnabledAsync(deviceId.ToString()).Result) // Check Mute Mic Enabled/Disable first
                            if (!_DTPProxyPlugin.GetIsWearDetectionMuteMicEnabledAsync(deviceId.ToString()).Result)
                            {
                                //_DeviceManagerPlugin.SetIsWearDetectionMuteMicEnabledAsync(deviceId.ToString(), true); // If Mute Mic Disable, need to turn On
                                _DTPProxyPlugin.SetIsWearDetectionMuteMicEnabledAsync(deviceId.ToString(), true);
                                _DTPProxyPlugin.SendHeadsetEventToUI($"HeadsetEvent_5;Device:Headset;EventType:Headset_WearDetectionChanged;DeviceId:{deviceId};Headset_IsWearDetectionMuteMicEnabledChanged:True");
                            }
                        }
                        else// If WearDetection need to turn Off
                        {
                            //_DeviceManagerPlugin.SetWearDetectionAsync(deviceId.ToString(), setvalue);
                            _DTPProxyPlugin.SetWearDetectionAsync(deviceId.ToString(), setvalue);
                        }
                        _DTPProxyPlugin.SendHeadsetEventToUI($"HeadsetEvent_5;Device:Headset;EventType:Headset_WearDetectionChanged;DeviceId:{deviceId};Headset_WearDetectionChanged:{setvalue.ToString()}");
                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.WearDetection = newValue;
                            writelog($"DTP:{nameof(SetWearDetectionForCLI)} device({device.Name}) SetWearDetectionAsync:{newValue}");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetWearDetectionForCLI)} device({device.Name}) exception with ({e.Message})");
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
                    try
                    {
                        //if (!_DeviceManagerPlugin.SetBusyLightAsync(deviceId.ToString(), newValue).Result)
                        if (!_DTPProxyPlugin.SetBusyLightAsync(deviceId.ToString(), newValue).Result)
                        {
                            _logicalDeviceHeadset.SetBusyLight(newValue);
                            writelog($"DTH:{nameof(SetBusyLight)} device({device.Name}) SetBusyLight:{newValue}");
                        }
                        else
                            writelog($"DTP:{nameof(SetBusyLight)} device({device.Name}) SetBusyLightAsync:{newValue}");
                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.BusyLight = newValue;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetBusyLight)} device({device.Name}) exception with ({e.Message})");
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
                    try
                    {
                        //if (!_DeviceManagerPlugin.SetVoiceGuidanceAsync(deviceId.ToString(), newValue).Result)
                        if (!_DTPProxyPlugin.SetVoiceGuidanceAsync(deviceId.ToString(), newValue).Result)
                        {
                            _logicalDeviceHeadset.SetVoiceGuidance(newValue);
                            writelog($"DTH:{nameof(SetVoiceGuidance)} device({device.Name}) SetVoiceGuidance:{newValue}");
                        }
                        else
                            writelog($"DTP:{nameof(SetVoiceGuidance)} device({device.Name}) SetVoiceGuidanceAsync:{newValue}");
                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.VoiceGuidance = newValue;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetVoiceGuidance)} device({device.Name}) exception with ({e.Message})");
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
                    try
                    {
                        //if (!_DeviceManagerPlugin.SetMicNCIncomingAsync(deviceId.ToString(), newValue).Result)
                        if (!_DTPProxyPlugin.SetMicNCIncomingAsync(deviceId.ToString(), newValue).Result)
                        {
                            _logicalDeviceHeadset.SetMicNCIncoming(newValue);
                            writelog($"DTH:{nameof(SetMicNCIncoming)} device({device.Name}) SetMicNCIncoming:{newValue}");
                        }
                        else
                            writelog($"DTP:{nameof(SetMicNCIncoming)} device({device.Name}) SetMicNCIncomingAsync:{newValue}");
                        DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                        if (_deviceInfo != null)
                        {
                            _deviceInfo.MicNCIncoming = newValue;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetMicNCIncoming)} device({device.Name}) exception with ({e.Message})");
                    }
                }
            }
        }

        public void SetAnswerCall(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                {
                    try
                    {
                        // IL add function at R16
                        //_DeviceManagerPlugin.SetBoomMicAsync(deviceId.ToString(), newValue);
                        _DTPProxyPlugin.SetBoomMicAsync(deviceId.ToString(), newValue);
                        writelog($"DTP:{nameof(SetAnswerCall)} device({device.Name}) SetBoomMicAsync:{newValue}");
                    }
                    catch (Exception e)
                    {
                        writelog($"DTP:{nameof(SetAnswerCall)} device({device.Name}) exception with ({e.Message})");
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
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
                        writelog($"DTH:{nameof(SetSideTopSwitchSinglePressSetting)} device({device.Name}) SideTopSwitchSinglePressSetting ok");
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.IsMicEnumerationOn = newValue;
                        writelog($"DTH:{nameof(SetIsMicEnumerationOn)} device({device.Name}) IsMicEnumerationOn:{newValue}");
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
                    //Debug.WriteLine($"{newValue}");
                    _iLogicalDeviceWebcam.ProfileManager.SetCurrentSelectedProfile(newValue);
                    writelog($"DTH:{nameof(SetCurrentSelectedProfile)} device({device.Name}) SetCurrentSelectedProfile:{newValue}");
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
                    // 2024-12-21, Elie R19 change that.
                    //_iLogicalDeviceWebcam.WALTime = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.WALTime = newValue;
                        writelog($"DTH:{nameof(SetWALTime)} device({device.Name}) WALTime:{newValue}");
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
                    // 2024-12-21, Elie R19 change that.
                    //_iLogicalDeviceWebcam.Snooze = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.Snooze = newValue;
                        writelog($"DTH:{nameof(SetSnooze)} device({device.Name}) Snooze:{newValue}");
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
                    //Debug.WriteLine($"{newValue}");
                    //_iLogicalDeviceWebcam.SetIsMicEnumerationOn(newValue);
                    //_iLogicalDeviceWebcam.SnoozeLength = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.SnoozeLength = newValue;
                        writelog($"DTH:{nameof(SetSnoozeLength)} device({device.Name}) SnoozeLength:{newValue}");
                        break;
                    }
                }
            }
        }

        //public void SetIsProximitySensorEnable(bool newValue, Guid deviceId)
        //{
        //    foreach (var device in _iDeviceManager.Devices)
        //    {
        //        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        //        if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
        //        {
        //            // 2024-12-21, Elie R19 change that.
        //            //_iLogicalDeviceWebcam.IsProximitySensorEnable = newValue;
        //            DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
        //            if (_deviceInfo != null)
        //            {
        //                _deviceInfo.IsProximitySensorEnable = newValue;
        //                writelog($"DTH:{nameof(SetIsProximitySensorEnable)} device({device.Name}) IsProximitySensorEnable:{newValue}");
        //                break;
        //            }
        //        }
        //    }
        //}

        public void SetIsWakeonApproachEnable(bool newValue, Guid deviceId)
        {
            foreach (var device in _iDeviceManager.Devices)
            {
                var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (logicalDevice is ILogicalDeviceWebcam _iLogicalDeviceWebcam)
                {
                    // 2024-12-21, Elie R19 change that.
                    //_iLogicalDeviceWebcam.IsWakeonApproachEnable = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.IsWakeonApproachEnable = newValue;
                        writelog($"DTH:{nameof(SetIsWakeonApproachEnable)} device({device.Name}) IsWakeonApproachEnable:{newValue}");
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
                    // 2024-12-21, Elie R19 change that.
                    //_iLogicalDeviceWebcam.IsWalkAwayLockEnable = newValue;
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                    if (_deviceInfo != null)
                    {
                        _deviceInfo.IsWalkAwayLockEnable = newValue;
                        writelog($"DTH:{nameof(SetIsWalkAwayLockEnable)} device({device.Name}) IsWalkAwayLockEnable:{newValue}");
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                    if (_deviceInfo != null)
                    {
                        //_deviceInfo.WALTime = newValue;
                        nRes = _deviceInfo.Snooze;
                        writelog($"DTH:{nameof(GetSnooze)} device({device.Name}) Snooze:{nRes}");
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
                    DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
                    if (_deviceInfo != null)
                    {
                        //_deviceInfo.WALTime = newValue;
                        nRes = _deviceInfo.SnoozeLength;
                        writelog($"DTH:{nameof(GetSnoozeLength)} device({device.Name}) SnoozeLength:{nRes}");
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
            writelog("PeripheralsPlugin plugin starting");

            PluginCondition = new PluginStartedCondition();
            //InitializeDeviceManagerPlugin();
        }

        #endregion

        #region Private Methods
        //move to CommonFunctions
        /*private bool IsServiceRunning()
        {
            //Bruce 0221 Add check service status 
            _logs.DebugMsg_1($"[PeripheralsPlugin] {nameof(IsServiceRunning)} start");
            bool ret = false;
            string serviceName = "DPMService";
            try
            {
                using (ServiceController service = new ServiceController(serviceName))
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        ret = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"[PeripheralsPlugin] {nameof(IsServiceRunning)} Error: {ex.Message}");
            }
            _logs.DebugMsg_1($"[PeripheralsPlugin] {nameof(IsServiceRunning)} done. ret : {ret}");
            return ret;
        }*/
        private void ScanDevices()
        {
            lock (_Lock)//this)
            {
                if (CommonFunctions.IsServiceRunning(GlobalDefinitions.DPeMServiceName, Log) && _isClientConnected && _iClient != null && _iDeviceManager != null)
                {
                    _DockCount = 0;
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
                    _IODongleCount_Gen3Ago = _iDeviceManager.Devices.ToList().FindAll(o => o.Type.Equals(DeviceType.PhysicalDongle) && o.Name.ToLower().Equals(GlobalDefinitions.Dongle_BeforeGen2_Name.ToLower())).Count;
                    _logs.DebugMsg_1("[PeripheralsPlugin] _IODongleCount_Gen3Ago : " + _IODongleCount_Gen3Ago);
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
                            // 2024-12-21, Elie R19 change that. (parameter has changed)               
                            physicalAudioDeviceDongle.PairingStatusChanged += PhysicalAudioDeviceDongle_PairingStatusChanged;
                            ;
                            PhysicalDevices2.Add(device.Id);
                        }
                        // >>

                        if (device is IPhysicalPenDevice penDevice)
                        {
                            _deviceHelper.IsdDriverVersion = penDevice.IsdServiceVersion;
                            if (!PhysicalPenDevices.Contains(device.Id))
                            {
                                PhysicalPenDevices.Add(penDevice.Id);
                                penDevice.ActivePenInformationChanged += PenDevice_ActivePenInformationChanged;
                            }

                        }

                        foreach (var item in device.Devices)
                        {
                            _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... Name " + item.Name);
                            _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... ModelNumber " + item.ModelNumber);
                            _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... BatteryLevel " + item.BatteryLevel);
                            _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... FirmwareVersion " + item.FirmwareVersion.ToString("X4"));
                            _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... PhysicalDeviceFirmwareVersion " + item.ParentPhysicalDevice.FirmwareVersion.ToString("X4"));
                            string newModel = JudgmentList.ModelRename(item.ModelNumber);
                            if (string.IsNullOrEmpty(newModel))
                            {
                                _logs.DebugMsg_1("[PeripheralsPlugin] DeviceInfo ... newModel " + newModel);
                                newModel = item.ModelNumber;
                            }
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
                                ModelNumber = newModel,
                                IsBatteryLevelSupported = item.IsBatteryLevelSupported,
                                ThumbnailImageRawData = item.ThumbnailImageRawData,
                            };

                            if (item.ParentPhysicalDevice.Type == DeviceType.PhysicalDongle &&
                                item.ParentPhysicalDevice is IPhysicalDeviceDongle _physicalDeviceDongle)
                            {
                                info.PairingStatusName = UpdateParingStausText(_physicalDeviceDongle.PairingStatus);
                                info.MaxPairingSlots = _physicalDeviceDongle.MaxPairingSlots;
                                info.PairedDeviceCount = _physicalDeviceDongle.PairedDeviceCount;
                                info.IsPhysicalDeviceDongle = true; //Because if it is a physical device dongle it will return true.                                
                            }

                            if (item.ParentPhysicalDevice.Type == DeviceType.PhysicalAudioDongle &&
                                item.ParentPhysicalDevice is IPhysicalAudioDeviceDongle _physicalAudioDeviceDongle)
                            {
                                info.PairingStatusName = UpdateParingStausText(_physicalAudioDeviceDongle.PairingStatus);
                                info.MaxPairingSlots = _physicalAudioDeviceDongle.MaxPairingSlots;
                                info.PairedDeviceCount = _physicalAudioDeviceDongle.PairedDeviceCount;
                                info.IsPhysicalDeviceDongle = false;
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
                                    physicalDevicePen.IsdVersionChanged += PhysicalDevicePen_IsdVersionChanged;
                                    if (!LogicalDevicesPen.Contains(pen.Id))
                                    {
                                        pen.PenSettingChanged += Pen_PenSettingChanged;
                                        pen.KeyCaptureStarted += Pen_KeyCaptureStarted;
                                        pen.KeyCaptureDataChanged += Pen_KeyCaptureDataChanged;
                                        pen.KeyCaptureProgressDataChanged += Pen_KeyCaptureProgressDataChanged;
                                        LogicalDevicesPen.Add(pen.Id);
                                    }
                                }
                                //Debug.Write($"TiltSensitivity: {info.TiltSensitivity}");
                                writelog($"TiltSensitivity: {info.TiltSensitivity}");
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

                                if (!LogicalDevices2.Contains(_logicalDevice2.Id))
                                {
                                    _logicalDevice2.DpiLevelChanged += ILogicalDevice_DpiLevelChanged;
                                    LogicalDevices2.Add(_logicalDevice2.Id);
                                }
                            }

                            if (item is ILogicalDevice3 _logicalDevice3)
                            {
                                // 2024-12-21, Elie R19 change that. (Using function calll to do that)
                                // << 2024-12-23 Updted by Hess
                                info.IsCollaborationBlinkEffectEnable = _logicalDevice3.IsCollaborationBlinkEffectEnable;
                                info.IsCollaborationCameraEnable = _logicalDevice3.IsCollaborationCameraEnable;
                                info.IsCollaborationChatEnable = _logicalDevice3.IsCollaborationChatEnable;
                                info.IsCollaborationDoubleTapEnable = _logicalDevice3.IsCollaborationDoubleTapEnable;
                                info.IsCollaborationKeyEnable = _logicalDevice3.IsCollaborationKeyEnable;
                                info.IsCollaborationMicEnable = _logicalDevice3.IsCollaborationMicEnable;
                                info.IsCollaborationScreenShareEnable = _logicalDevice3.IsCollaborationScreenShareEnable;
                                // >>
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

                                if (!LogicalDevices3.Contains(_logicalDevice3.Id))
                                {
                                    _logicalDevice3.MousePrimaryButtonChanged += ILogicalDevice_MousePrimaryButtonChanged;
                                    _logicalDevice3.DPIValueChanged += ILogicalDevice_DpiValueChanged;
                                    _logicalDevice3.TouchScrollSensitivityLevelChanged += ILogicalDevice_TouchScrollSensitivityLevelChanged;
                                    _logicalDevice3.BackLightingControlsChanged += ILogicalDevice_BackLightingControlsChanged;
                                    _logicalDevice3.BackLightingLevelChanged += ILogicalDevice_BackLightingLevelChanged;
                                    _logicalDevice3.PairedHostNameChanged += ILogicalDevice_PairedHostNameChanged;
                                    _logicalDevice3.IsDPILevelChangePendingChanged += ILogicalDevice_IsDPILevelChangePendingChanged;
                                    _logicalDevice3.IsDPIValueChangePendingChanged += ILogicalDevice_IsDPIValueChangePendingChanged;
                                    _logicalDevice3.ReportRateChanged += _logicalDevice3_ReportRateChanged;
                                    LogicalDevices3.Add(_logicalDevice3.Id);
                                }
                            }

                            // << 241206 by Hess fix no event issue
                            if (item is ILogicalDevice _logicalDevice &&
                                !LogicalDevices.Contains(_logicalDevice.Id))
                            {
                                _logicalDevice.BatteryStatusChanged += ILogicalDevice_BatteryStatusChanged;
                                _logicalDevice.BatteryLevelChanged += ILogicalDevice_BatteryLevelChanged;
                                LogicalDevices.Add(_logicalDevice.Id);

                                CheckLowBatteryOSD(info);
                            }
                            // >>
                            // << 241228 by Hess add new event
                            if (item is IDevice _IDevice &&
                                !IDevices.Contains(_IDevice.Id))
                            {
                                device.NameChanged += (name) => OnDeviceNameChanged(device, name);
                                IDevices.Add(_IDevice.Id);
                            }
                            // >>

                            if (item is ILogicalWiredAudio _logicalWiredAudio)
                            {
                                info.MuteStatus = _logicalWiredAudio.MuteStatus;
                                //info.IsWiredAudioIMicNSEnable = _logicalWiredAudio.IsWiredAudioIMicNSEnable();
                                //info.IsWiredAudioMicMuteSoundEnable = _logicalWiredAudio.IsWiredAudioMicMuteSoundEnable();
                                //info.WiredAudioVolumeAdjustmentTone = _logicalWiredAudio.GetWiredAudioVolumeAdjustmentTone();
                                var wiredAudio = (ILogicalWiredAudio)item;
                                _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalWiredAudio ScanDevices Add DTH event before ...  ");
                                if (!LogicalWiredAudio.Contains(wiredAudio.Id))
                                {
                                    _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalWiredAudio Add Event ID : {wiredAudio.Id.ToString()}, in... ");
                                    _logicalWiredAudio.MuteStatusChanged += ILogicalWiredAudio_MuteStatusChanged;
                                    LogicalWiredAudio.Add(wiredAudio.Id);
                                    _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalWiredAudio Add Event ID : {wiredAudio.Id.ToString()}, out... ");
                                }
                                _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalWiredAudio ScanDevices Add DTH event after ...  ");
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
                                info.SelectedResolution = Encoding.UTF8.GetString(_iLogicalDeviceWebcam.GetSelectedResolution());
                                info.TiltMax = _iLogicalDeviceWebcam.TiltMax;
                                info.TiltMin = _iLogicalDeviceWebcam.TiltMin;
                                info.TiltSteppingDelta = _iLogicalDeviceWebcam.TiltSteppingDelta;
                                info.WhiteBalanceMax = _iLogicalDeviceWebcam.WhiteBalanceMax;
                                info.WhiteBalanceMin = _iLogicalDeviceWebcam.WhiteBalanceMin;
                                info.WhiteBalanceSteppingDelta = _iLogicalDeviceWebcam.WhiteBalanceSteppingDelta;
                                info.ZoomMax = _iLogicalDeviceWebcam.ZoomMax;
                                info.ZoomMin = _iLogicalDeviceWebcam.ZoomMin;
                                info.ZoomSteppingDelta = _iLogicalDeviceWebcam.ZoomSteppingDelta;
                                info.IsPrioritizeExternalWebcam = _iLogicalDeviceWebcam.IsPrioritizeExternalWebcam;
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
                                _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceHeadset ScanDevices Add DTH value ... in ");
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
                                var headset = (ILogicalDeviceHeadset)item;
                                _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceHeadset ScanDevices Add DTH event before ...  ");
                                if (!LogicalDevicHeadset.Contains(headset.Id))
                                {
                                    _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceHeadset Add Event ID : {headset.Id.ToString()}, in... ");
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
                                    LogicalDevicHeadset.Add(headset.Id);
                                    _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceHeadset Add Event ID : {headset.Id.ToString()}, out... ");
                                }
                                _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceHeadset ScanDevices Add DTH event after ...  ");
                            }

                            if (item is ILogicalDeviceDock _logicalDeviceDock)
                            {
                                _DockCount++;
                                info.DockInfo = _logicalDeviceDock.GetDockInfo();
                                try
                                {
                                    //IDevice iDevice = (IDevice)item;
                                    //if (iDevice != null)
                                    //{
                                    //    info.FirmwareVersion = iDevice.FirmwareVersion.ToString();
                                    //}
                                    info.DockPackageFwVersion = info.FirmwareVersion;
                                    if (!string.IsNullOrEmpty(info.DockPackageFwVersion))
                                    {
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] info.DockPackageFwVersion befor = {info.DockPackageFwVersion}");
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] info.FirmwareVersion befor = {info.FirmwareVersion}");
                                        info.DockPackageFwVersion = info.DockPackageFwVersion.PadLeft(8, '0');
                                        info.FirmwareVersion = info.FirmwareVersion.PadLeft(8, '0');
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] info.DockPackageFwVersion after = {info.DockPackageFwVersion}");
                                        _logs.DebugMsg_1($"[PeripheralsPlugin] info.FirmwareVersion after = {info.FirmwareVersion}");
                                    }
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
                                    /*Bruce 01/10 註解，因有把Dock API 卡住的疑慮
                                     * dokc_bytes = _logicalDeviceDock.GetDockData();
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
                                                        if (string.IsNullOrEmpty(info.DockServiceTag) && !string.IsNullOrEmpty(dockData.ServiceTag))
                                                        {
                                                            info.DockServiceTag = dockData.ServiceTag;
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logs.DebugMsg_1($"[PeripheralsPlugin] GetDockData Error : {ex.Message}");
                                            }
                                        }
                                    }*/
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
                                    /*Bruce 01/10 註解，因有把Dock API 卡住的疑慮
                                     * dokc_bytes = _logicalDeviceDock.GetDockTBTConnectionStatus();
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
                                    }*/
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
                    //Console.WriteLine(_deviceHelper.ToString());
                    writelog(_deviceHelper.ToString());
                    CheckDocks();
                    writelog($"after CheckDocks --- {_deviceHelper.ToString()}");
                    OnUpdateNotify(true);//Bruce PIMS-346696 Because the DeviceInfo event is slower than the Update event, resulting in incomplete DeviceInfo, the Updater event is moved here.
                }
            }
        }

        private void _logicalDevice3_ReportRateChanged(ILogicalDevice3 arg1, int arg2)
        {
            writelog($"ReportRateChanged: Guid:{arg1.Id}  NewValue:{arg2}");
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.ReportRate = arg2;

                    DeviceChangedEventArgs _EventArgs = new()
                    {
                        type = DeviceChangedType.Peripherals_SettingsChange,
                        device_peripherals = deviceInfo,
                        changedProperty = "ReportRateChanged"
                    };
                    OnNotify(_EventArgs);
                }
                else
                {
                    writelog($"ReportRateChanged: Error: deviceInfo is null");
                }
            }
        }

        private void PenDevice_ActivePenInformationChanged(IPhysicalPenDevice arg1, string arg2, string arg3, string arg4, bool arg5, bool arg6, bool arg7, int arg8)
        {
            writelog($"ActivePenInformationChanged: Guid:{arg1.Id} PenID:{arg1.PenId} arg2:{arg2} arg3:{arg3} arg4:{arg4} IsSupported:{arg5} IsConnected:{arg6} arg7:{arg7} arg8:{arg8}");
            var deviceInfo = new DeviceInfo
            {
                IsBLE = !string.IsNullOrEmpty(arg4),
                IsConnected = arg6,
                IsReady = arg5
            };
            DeviceChangedEventArgs _EventArgs = new()
            {
                type = DeviceChangedType.Peripherals_SettingsChange,
                device_peripherals = deviceInfo,
                changedProperty = "ActivePenInformationChanged"
            };
            OnNotify(_EventArgs);
        }

        private void OnDeviceNameChanged(IDevice device, string newValue)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == device.Id.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.Name = newValue;

                    DeviceChangedEventArgs _EventArgs = new()
                    {
                        type = DeviceChangedType.Peripherals_SettingsChange,
                        device_peripherals = deviceInfo,
                        changedProperty = "DeviceNameChanged"
                    };
                    OnNotify(_EventArgs);
                    Debug.WriteLine($"DeviceNameChanged: ID: {device.Id} Name: {newValue}");
                    writelog($"DeviceNameChanged: ID: {device.Id} Name: {newValue}");
                }
                else
                {
                    writelog($"DeviceNameChanged: Error: deviceInfo is null");
                }
            }
        }


        private void _logicalDevice_NameChanged(string name)
        {
        }

        private void ILogicalDevice_IsDPIValueChangePendingChanged(ILogicalDevice3 arg1, bool arg2)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                deviceInfo ??= new()
                {
                    ID = arg1.Id
                };
                deviceInfo.IsDPIValueChangePending = arg2;

                //Debug.WriteLine($"DPIValueChangePendingChanged: Guid:{arg1.Id} Value: {arg2}");
                writelog($"DPIValueChangePendingChanged: Guid:{arg1.Id} Value: {arg2}");

                DeviceChangedEventArgs _EventArgs = new()
                {
                    type = DeviceChangedType.Peripherals_SettingsChange,
                    device_peripherals = deviceInfo,
                    changedProperty = "DPIValueChangePendingChanged"
                };
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalDevice_IsDPILevelChangePendingChanged(ILogicalDevice3 arg1, bool arg2)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                deviceInfo ??= new()
                {
                    ID = arg1.Id
                };
                deviceInfo.IsDPILevelChangePending = arg2;

                //Debug.WriteLine($"DPILevelChangePendingChanged: Guid:{arg1.Id} Value: {arg2}");
                writelog($"DPILevelChangePendingChanged: Guid:{arg1.Id} Value: {arg2}");

                DeviceChangedEventArgs _EventArgs = new()
                {
                    type = DeviceChangedType.Peripherals_SettingsChange,
                    device_peripherals = deviceInfo,
                    changedProperty = "DPILevelChangePendingChanged"
                };
                OnNotify(_EventArgs);
            }
        }

        private void Pen_KeyCaptureProgressDataChanged(ILogicalDevicePen arg1, string arg2)
        {
            //Debug.WriteLine($"Pen: {arg1.Id} KeyCaptureProgressDataChangedString, newValue: {arg2}");
            writelog($"Pen: {arg1.Id} KeyCaptureProgressDataChanged, newValue: {arg2}");
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo != null)
                {
                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = $"PenKeyCaptureProgressDataChanged|{arg2}";
                    OnNotify(_EventArgs);
                }
                else
                {
                    writelog($"PenKeyCaptureProgressDataChanged: Error: deviceInfo is null");
                }
            }
        }

        private void Pen_KeyCaptureDataChanged(ILogicalDevicePen arg1, string arg2)
        {
            //Debug.WriteLine($"Pen: {arg1.Id} KeyCaptureDataChangedString, newValue: {arg2}");
            writelog($"Pen: {arg1.Id} KeyCaptureDataChanged, newValue: {arg2}");
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo != null)
                {
                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "PenKeyCaptureDataChanged";
                    OnNotify(_EventArgs);
                }
                else
                {
                    writelog($"PenKeyCaptureDataChanged: Error: deviceInfo is null");
                }
            }
        }

        private void Pen_KeyCaptureStarted(ILogicalDevicePen obj)
        {
            //Debug.WriteLine($"Pen: {obj.Id} KeyCaptureStarted!");
            writelog($"Pen: {obj.Id} KeyCaptureStarted!");
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == obj.Id.ToString());
                if (deviceInfo != null)
                {
                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "PenKeyCaptureStarted";
                    OnNotify(_EventArgs);
                }
                else
                {
                    writelog($"PenKeyCaptureStarted: Error: deviceInfo is null");
                }
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
                else
                {
                    writelog($"PairedHostNameChanged: Error: deviceInfo is null");
                }
            }
        }

        private void Pen_PenSettingChanged(ILogicalDevicePen arg1, string arg2)
        {
            //Debug.WriteLine(arg2);
            writelog(arg2);
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo != null)
                {
                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "PenSettingChanged";
                    OnNotify(_EventArgs);
                }
                else
                {
                    writelog($"PenSettingChanged: Error: deviceInfo is null");
                }
            }
        }

        private void FillRFDeviceInfo(IPhysicalDevice device)
        {
            if (device is IPhysicalAudioDeviceDongle _iPhysicalAudioDeviceDongle)
            {
                DongleInfo rfInfo = new DongleInfo();
                var rfdongle = _rfDeviceHelper.dongleInfo.FirstOrDefault(x => x.DeviceType == _iPhysicalAudioDeviceDongle.Type);
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
                var rfdongle = _rfDeviceHelper.dongleInfo.FirstOrDefault(x => x.DeviceType == _iPhysicalDeviceDongle.Type);
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
            writelog($"UpdateDonglePairingStatus ... in ");
            switch (donglePairingStatus)
            {
                case DonglePairingStatus.DonglePairingStatusStopped:
                    writelog($"Stopped : DonglePairingStatusStopped");
                    return "Stopped";

                case DonglePairingStatus.DonglePairingStatusStarted:
                    writelog($"Started : DonglePairingStatusStopped");
                    return "Started";

                case DonglePairingStatus.DonglePairingStatusRequest:
                    writelog($"Request : DonglePairingStatusStopped");
                    return "Request";

                case DonglePairingStatus.DonglePairingStatusTimeOut:
                    writelog($"TimeOut : DonglePairingStatusStopped");
                    return "TimeOut";

                case DonglePairingStatus.DonglePairingStatusAlreadyPaired:
                    writelog($"Already Paired : DonglePairingStatusStopped");
                    return "Already Paired";

                case DonglePairingStatus.DonglePairingStatusOldDevice:
                    writelog($"Old Device : DonglePairingStatusStopped");
                    return "Old Device";
            }
            writelog($"Empty : DonglePairingStatus Return Empty");
            return "";
        }

        private string UpdateParingStausText(AudioDonglePairingStatus donglePairingStatus)
        {
            writelog($"UpdateAudioDonglePairingStatusText ... in ");
            switch (donglePairingStatus)
            {
                case AudioDonglePairingStatus.AudioDonglePairingStatusStopped:
                    writelog($"Stopped : AudioDonglePairingStatusRequest");
                    return "Stopped";

                case AudioDonglePairingStatus.AudioDonglePairingStatusStarted:
                    writelog($"Started : AudioDonglePairingStatusRequest");
                    return "Started";

                case AudioDonglePairingStatus.AudioDonglePairingStatusRequest:
                    writelog($"Request : AudioDonglePairingStatusRequest");
                    return "Request";

                case AudioDonglePairingStatus.AudioDonglePairingStatusTimeOut:
                    writelog($"TimeOut : AudioDonglePairingStatusTimeOut");
                    return "TimeOut";

                case AudioDonglePairingStatus.AudioDonglePairingStatusAlreadyPaired:
                    writelog($"Already Paired : AudioDonglePairingStatusAlreadyPaired");
                    return "Already Paired";
            }
            writelog($"Empty : UpdateAudioDonglePairingStatusText Return Empty");
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
            Task.Run(() =>
            {
                if (Notify != null)
                    Notify(this, e);
            });
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
            if (_iDeviceManager != null)
            {
                writelog($"Client_StatusEvent: {_iDeviceManager.Devices.Count.ToString()}; _isClientConnected = {_isClientConnected.ToString()}");
            }
            if (status == ClientStatus.Connected)
            {
                lock (_Lock)//this)
                {
                    writelog($"_isClientConnected turn true ... ");
                    _isClientConnected = true;

                    _iClient = client;
                    _iDeviceManager = _iClient.DeviceManager;
                    _iDeviceManager.DeviceAddedEvent += _iDeviceManager_DeviceAddedEvent;
                    _iDeviceManager.DeviceRemovedEvent += _iDeviceManager_DeviceRemovedEvent;
                    _iClient.RawInputManager.DisplayDataChanged += RawInputManager_DisplayDataChanged;
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
                lock (_Lock)//this)
                {
                    if (_isClientConnected)
                    {
                        writelog($"_isClientConnected turn false ... ");
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
            //Console.WriteLine(_clientInfo.ToString());
            //Debug.WriteLine(_clientInfo.ToString());
            writelog(_clientInfo.ToString());
        }

        private void RawInputManager_DisplayDataChanged(string obj)
        {
            writelog($"KeyStroke DisplayDataChanged: value:{obj}");
            DeviceInfo di = new()
            {
                Message = obj
            };
            DeviceChangedEventArgs _EventArgs = new()
            {
                type = DeviceChangedType.Peripherals_SettingsChange,
                device_peripherals = di,
                changedProperty = "KeyStrokeDisplayDataChanged"
            };
            OnNotify(_EventArgs);
        }

        private void _iCTKMessageHelper_IsZoomCallbacksRegisteredChanged(bool obj)
        {
            IsZoomCallbacksRegisteredChanged?.Invoke(this, obj);
        }

        private void _iCTKMessageHelper_IsZoomMultipleCallsDetectedChanged(bool obj)
        {
            IsZoomMultipleCallsDetectedChanged?.Invoke(this, obj);

            if (obj)
            {
                //_ = _DeviceManagerPlugin.ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.CollaborationNotAvailable, OSDType_Device.Keyboard, LangHelper.Instance["CollabMultipleCalls"]);
                OSDEventArgs args = new()
                {
                    Requester = "CollaborationNotAvailable.Keyboard",
                    DeviceName = Screen.PrimaryScreen.DeviceName,
                    osd_type = OSDType.CollaborationNotAvailable,
                    osd_device = OSDType_Device.Keyboard,
                    Message = LangHelper.Instance["CollabMultipleCalls"]
                };
                OnOSDNotify(args);
            }
        }

        private void _iCTKMessageHelper_CollabMultipleCallsDetectedChanged(bool obj)
        {
            CollabMultipleCallsDetectedChanged?.Invoke(this, obj);
            Debug.WriteLine($"CollabMultipleCallsDetectedChanged: {obj}");
            writelog($"CollabMultipleCallsDetectedChanged: {obj}");

            if (obj)
            {
                //_ = _DeviceManagerPlugin.ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.CollaborationNotAvailable, OSDType_Device.Keyboard, LangHelper.Instance["CollabMultipleCalls"]);
                OSDEventArgs args = new()
                {
                    Requester = "CollaborationNotAvailable.Keyboard",
                    DeviceName = Screen.PrimaryScreen.DeviceName,
                    osd_type = OSDType.CollaborationNotAvailable,
                    osd_device = OSDType_Device.Keyboard,
                    Message = LangHelper.Instance["CollabMultipleCalls"]
                };
                OnOSDNotify(args);
            }
        }

        private void _iCTKMessageHelper_CollaborationMsgChanged(CollaborationMsg collaborationMsg)
        {
            Debug.WriteLine($"CollaborationMsgChanged: {collaborationMsg.ToString() ?? ""}");
            writelog($"CollaborationMsgChanged: {collaborationMsg.ToString() ?? ""}");
            //CollaborationMsgNotify?.Invoke(EventArgs.Empty, collaborationMsg);
            var di = new DeviceInfo
            {
                Message = collaborationMsg.ToString()
            };
            DeviceChangedEventArgs _EventArgs = new()
            {
                type = DeviceChangedType.Peripherals_SettingsChange,
                changedProperty = "CollaborationMsgChanged",
                device_peripherals = di
            };
            OnNotify(_EventArgs);
        }

        private void _iOverlayManager_VolatileSettingsChanged(bool arg1, string arg2, string arg3)
        {
            OverlayNotify?.Invoke(EventArgs.Empty, new Tuple<string, string>(arg2, arg3));
        }

        private void _iDeviceManager_DeviceAddedEvent(IPhysicalDevice iPhysicalDevice)
        {
            if (PhysicalDevices.Contains(iPhysicalDevice.Id))
            { return; }

            //System.Diagnostics.Debug.WriteLine("ParentPhysicalDevice Added, Id : " + iPhysicalDevice.Id + ", Name : " + iPhysicalDevice.Name);
            writelog("ParentPhysicalDevice Added, Id : " + iPhysicalDevice.Id + ", Name : " + iPhysicalDevice.Name);

            lock (_Lock)//this)
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

        private void _iDeviceManager_DeviceRemovedEvent(Guid physicalDeviceId)//(IPhysicalDevice iPhysicalDevice) // Elie, 25/02/17 R23 changes the interface
        {
            //System.Diagnostics.Debug.WriteLine("ParentPhysicalDevice Removed, Id : " + iPhysicalDevice.Id + ", Name : " + iPhysicalDevice.Name);
            //writelog("ParentPhysicalDevice Removed, Id : " + iPhysicalDevice.Id + ", Name : " + iPhysicalDevice.Name);
            writelog("ParentPhysicalDevice Removed, Id : " + physicalDeviceId);// + ", Name : " + iPhysicalDevice.Name);
            lock (_PeripheralLock)
            {
                if (_isClientConnected)
                {
                    //IPhysicalDevice iPhysicalDevice;
                    List<IPhysicalDevice> iPhysicalDevices = _iDeviceManager.Devices
                        .Where(x => x.Id == physicalDeviceId)
                        .ToList();

                    if (iPhysicalDevices == null || iPhysicalDevices.Count == 0)
                    {
                        writelog($"[DeviceRemovedEvent]No devices found with ID: {physicalDeviceId}");
                    }
                    else
                    {
                        foreach (var iPhysicalDevice in iPhysicalDevices)
                        {
                            writelog("iPhysicalDevice.Name = " + iPhysicalDevice.Name.ToString() + ", iPhysicalDevice.ModelNumber = " + iPhysicalDevice.ModelNumber.ToString());
                            iPhysicalDevice.DeviceAddedEvent -= IPhysicalDevice_DeviceAddedEvent;
                            iPhysicalDevice.DeviceRemovedEvent -= IPhysicalDevice_DeviceRemovedEvent;

                            if (iPhysicalDevice is IPhysicalPenDevice penDevice && PhysicalPenDevices.Contains(penDevice.Id))
                            {
                                penDevice.ActivePenInformationChanged -= PenDevice_ActivePenInformationChanged;
                                PhysicalPenDevices.Remove(penDevice.Id);
                            }

                            ScanDevices();

                            //if (iPhysicalDevice.Type == DeviceType.PhysicalAudioDongle || iPhysicalDevice.Type == DeviceType.PhysicalDongle)
                            //{
                            //    DeviceChangedEventArgs _EventArgs = new();
                            //    _EventArgs.type = DeviceChangedType.Peripherals_UnPlug;
                            //    _EventArgs.changedProperty = "PhysicalDeviceRemoved";
                            //    OnNotify(_EventArgs);
                            //}
                        }
                    }
                    DeviceChangedEventArgs _EventArgs = new()
                    {
                        type = DeviceChangedType.Peripherals_UnPlug,
                        changedProperty = "PhysicalDeviceRemoved"
                    };
                    OnNotify(_EventArgs);
                }

                // << 250320 added by Hess
                PhysicalDevices.Remove(physicalDeviceId);
                PhysicalDevices2.Remove(physicalDeviceId);
                PhysicalPenDevices.Remove(physicalDeviceId);
                // >>
            }
        }

        private void IPhysicalDevice_DeviceAddedEvent(ILogicalDevice iLogicalDevice)
        {
            //System.Diagnostics.Debug.WriteLine("LogicalDevice Added, Id : " + iLogicalDevice.Id + ", Name : " + iLogicalDevice.Name);
            if (string.IsNullOrEmpty(iLogicalDevice.Name))
                writelog("LogicalDevice Added, Id : " + iLogicalDevice.Id + ", Name : NULL (iLogicalDevice.Name)");
            else
                writelog("LogicalDevice Added, Id : " + iLogicalDevice.Id + ", Name : " + iLogicalDevice.Name);

            lock (_Lock)//this)
            {
                if (_isClientConnected)
                {
                    ScanDevices();

                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_PlugIn;
                    _EventArgs.device_peripherals = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == iLogicalDevice.Id.ToString());
                    _EventArgs.changedProperty = "LogicalDeviceAdded";
                    OnNotify(_EventArgs);
                    writelog($"LogicalDeviceAdded event published, Id: {iLogicalDevice.Id}, Name: {iLogicalDevice.Name ?? ""}");
                }
                else
                {
                    writelog($"LogicalDeviceAdded event bypassed, Id: {iLogicalDevice.Id}, Name: {iLogicalDevice.Name ?? ""}");
                }
            }
        }

        private void IPhysicalDevice_DeviceRemovedEvent(Guid deviceGuid)//(ILogicalDevice iLogicalDevice) // Elie, 25/02/17 R23 changes the interface
        {
            if (deviceGuid != Guid.Empty)
            {
                writelog("ParentPhysicalDevice Removed, Id : " + deviceGuid.ToString());
            }
            lock (_Lock)//this)
            {
                if (_isClientConnected)
                {
                    //Debug.WriteLine($"ID: {iLogicalDevice.Id}, Type:{iLogicalDevice.Type}");
                    //Debug.WriteLine($"DeviceCount: {_deviceHelper.deviceInfo.Count}");
                    //writelog($"ID: {iLogicalDevice.Id}, Type:{iLogicalDevice.Type}");
                    writelog($"ID: {deviceGuid}");//, Type:{iLogicalDevice.Type}");
                    writelog($"DeviceCount: {_deviceHelper.deviceInfo.Count}");

                    List<DeviceInfo> deviceInfoList = _deviceHelper.deviceInfo.Where(x => x.ID == deviceGuid).ToList();

                    deviceInfoList.ForEach(device =>
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

                    //_deviceHelper.deviceInfo.Where(x => x.ID == deviceGuid).ToList().ForEach(device =>
                    //{
                    //    device.IsConnected = false;

                    //    DeviceChangedEventArgs _EventArgs = new()
                    //    {
                    //        type = DeviceChangedType.Peripherals_UnPlug,
                    //        device_peripherals = device,
                    //        changedProperty = "LogicalDeviceRemoved"
                    //    };
                    //    OnNotify(_EventArgs);
                    //});

                    ILogicalDevice iLogicalDevice = _iDeviceManager.Devices.SelectMany(device => device.Devices).FirstOrDefault(x => x.Id == deviceGuid);

                    if (iLogicalDevice == null)
                    {
                        writelog($"***** Device not found by GUID: {deviceGuid}, it cannot Remove DTP Event. *****");
                    }
                    else
                    {
                        if (IDevices.Contains(iLogicalDevice.Id) && iLogicalDevice is IDevice _IDevice)
                        {
                            _IDevice.NameChanged -= (name) => OnDeviceNameChanged(_IDevice, name);
                            IDevices.Remove(iLogicalDevice.Id);
                        }
                        if (LogicalDevices.Contains(iLogicalDevice.Id) && iLogicalDevice is ILogicalDevice _logicalDevice)
                        {
                            _logicalDevice.BatteryStatusChanged -= ILogicalDevice_BatteryStatusChanged;
                            _logicalDevice.BatteryLevelChanged -= ILogicalDevice_BatteryLevelChanged;
                            LogicalDevices.Remove(iLogicalDevice.Id);
                        }
                        if (LogicalDevices2.Contains(iLogicalDevice.Id) && iLogicalDevice is ILogicalDevice2 _logicalDevice2)
                        {
                            _logicalDevice2.DpiLevelChanged -= ILogicalDevice_DpiLevelChanged;
                            LogicalDevices2.Remove(iLogicalDevice.Id);
                        }
                        if (LogicalDevices3.Contains(iLogicalDevice.Id) && iLogicalDevice is ILogicalDevice3 _logicalDevice3)
                        {
                            _logicalDevice3.MousePrimaryButtonChanged -= ILogicalDevice_MousePrimaryButtonChanged;
                            _logicalDevice3.DPIValueChanged -= ILogicalDevice_DpiValueChanged;
                            _logicalDevice3.TouchScrollSensitivityLevelChanged -= ILogicalDevice_TouchScrollSensitivityLevelChanged;
                            _logicalDevice3.BackLightingControlsChanged -= ILogicalDevice_BackLightingControlsChanged;
                            _logicalDevice3.BackLightingLevelChanged -= ILogicalDevice_BackLightingLevelChanged;
                            _logicalDevice3.PairedHostNameChanged -= ILogicalDevice_PairedHostNameChanged;
                            _logicalDevice3.IsDPILevelChangePendingChanged -= ILogicalDevice_IsDPILevelChangePendingChanged;
                            _logicalDevice3.IsDPIValueChangePendingChanged -= ILogicalDevice_IsDPIValueChangePendingChanged;
                            _logicalDevice3.ReportRateChanged -= _logicalDevice3_ReportRateChanged;
                            LogicalDevices3.Remove(iLogicalDevice.Id);
                        }
                        if (LogicalDevicesPen.Contains(iLogicalDevice.Id) && iLogicalDevice is ILogicalDevicePen _logicalDevicePen)
                        {
                            _logicalDevicePen.PenSettingChanged -= Pen_PenSettingChanged;
                            _logicalDevicePen.KeyCaptureStarted -= Pen_KeyCaptureStarted;
                            _logicalDevicePen.KeyCaptureDataChanged -= Pen_KeyCaptureDataChanged;
                            _logicalDevicePen.KeyCaptureProgressDataChanged -= Pen_KeyCaptureProgressDataChanged;
                            LogicalDevicesPen.Remove(iLogicalDevice.Id);
                        }
                        _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceHeadset Remove DTH Event before ...  ");
                        if (LogicalDevicHeadset.Contains(iLogicalDevice.Id) && iLogicalDevice is ILogicalDeviceHeadset _logicalDeviceHeadset)
                        {
                            _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceHeadset Remove Event ID : {iLogicalDevice.Id.ToString()}, in ... ");
                            _logicalDeviceHeadset.IsReadyChanged -= _logicalDeviceHeadset_IsReadyChanged;
                            _logicalDeviceHeadset.IsDirtyChanged -= _logicalDeviceHeadset_IsDirtyChanged;
                            _logicalDeviceHeadset.MicNoiseCancellationChanged -= _logicalDeviceHeadset_MicNoiseCancellationChanged;
                            _logicalDeviceHeadset.MicNCIncomingChanged -= _logicalDeviceHeadset_MicNCIncomingChanged;
                            _logicalDeviceHeadset.SidetoneChanged -= _logicalDeviceHeadset_SidetoneChanged;
                            _logicalDeviceHeadset.BusyLightChanged -= _logicalDeviceHeadset_BusyLightChanged;
                            _logicalDeviceHeadset.VoiceGuidanceChanged -= _logicalDeviceHeadset_VoiceGuidanceChanged;
                            _logicalDeviceHeadset.SelectedPresetChanged -= _logicalDeviceHeadset_SelectedPresetChanged;
                            _logicalDeviceHeadset.SidetoneLevelChanged -= _logicalDeviceHeadset_SidetoneLevelChanged;
                            _logicalDeviceHeadset.MuteStatusChanged -= _logicalDeviceHeadset_MuteStatusChanged;
                            _logicalDeviceHeadset.BandsGainChanged -= _logicalDeviceHeadset_BandsGainChanged;
                            _logicalDeviceHeadset.AncModeChanged -= _logicalDeviceHeadset_AncModeChanged;
                            _logicalDeviceHeadset.AncGainChanged -= _logicalDeviceHeadset_AncGainChanged;
                            LogicalDevicHeadset.Remove(iLogicalDevice.Id);
                            _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceHeadset Remove Event ID : {iLogicalDevice.Id.ToString()}, out ... ");
                        }
                        _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceWiredAudio Remove DTH Event after ...  ");

                        _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceWiredAudio Remove DTH Event before ...  ");
                        if (LogicalWiredAudio.Contains(iLogicalDevice.Id) && iLogicalDevice is ILogicalWiredAudio _logicalDeviceWiredAudio)
                        {
                            _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceWiredAudio Remove Event ID : {iLogicalDevice.Id.ToString()}, in ... ");
                            _logicalDeviceWiredAudio.MuteStatusChanged -= ILogicalWiredAudio_MuteStatusChanged;
                            LogicalWiredAudio.Remove(iLogicalDevice.Id);
                            _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceWiredAudio Remove Event ID : {iLogicalDevice.Id.ToString()}, out ... ");
                        }
                        _logs.DebugMsg_1($"[PeripheralsPlugin] ILogicalDeviceWiredAudio Remove DTH Event after ...  ");
                    }

                    // << 250218 added by Hess for PIMS-328225
                    if (LowBatteryIDs.TryGetValue(deviceGuid.ToString(), out OSDType_Device type))
                    {
                        OSDEventArgs args = new()
                        {
                            Requester = "CloseBatteryLowOSD",
                            osd_type = OSDType.BatteryLow,
                            osd_device = type,
                            Guid = deviceGuid
                        };
                        OnOSDNotify(args);
                    }
                    // >>

                    ScanDevices();
                    var IDs = _deviceHelper.deviceInfo.Select(x => x.ID.ToString()).ToList();
                    writelog($"[PeripheralsPlugin] IPhysicalDevice_DeviceRemovedEvent LowBatteryIDs.Count : {LowBatteryIDs.Count}, IDs.Count : {IDs.Count} ... ");
                    var lbIDs = LowBatteryIDs.Keys.ToList();
                    for (int i = lbIDs.Count - 1; i >= 0; i--)
                    {
                        if (!IDs.Contains(lbIDs[i]))
                        {
                            LowBatteryIDs.Remove(lbIDs[i]);
                        }
                    }
                }

                // << 250320 added by Hess
                IDevices.Remove(deviceGuid);
                LogicalDevices.Remove(deviceGuid);
                LogicalDevices2.Remove(deviceGuid);
                LogicalDevices3.Remove(deviceGuid);
                LogicalDevicesPen.Remove(deviceGuid);
                LogicalDevicHeadset.Remove(deviceGuid);
                LogicalDevicHeadset.Remove(deviceGuid);
                // >>
            }
        }

        private void ILogicalDevice_DpiLevelChanged(ILogicalDevice2 arg1, int arg2)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo == null)
                {
                    writelog($"DpiLevelChanged: Error: deviceInfo is null");
                    return;
                }
                if (arg2 == 0)
                    return;
                //deviceInfo.DpiLevel = arg2 - 1;
                deviceInfo.DpiLevel = arg2;
                Debug.WriteLine($"DpiLevelChanged: New DpiLevel: {arg2}");
                writelog($"DpiLevelChanged: New DpiLevel: {arg2}");
                DeviceChangedEventArgs _EventArgs = new()
                {
                    type = DeviceChangedType.Peripherals_SettingsChange,
                    device_peripherals = deviceInfo,
                    changedProperty = "DpiLevelChanged"
                };
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalDevice_DpiValueChanged(ILogicalDevice3 arg1, int arg2)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo == null)
                {
                    writelog($"DpiValueChanged: Error: deviceInfo is null");
                    return;
                }
                deviceInfo.DpiValue = arg2.ToString();
                Debug.WriteLine($"DpiValueChanged: New DpiValue: {arg2}");
                writelog($"DpiValueChanged: New DpiValue: {arg2}");
                DeviceChangedEventArgs _EventArgs = new()
                {
                    type = DeviceChangedType.Peripherals_SettingsChange,
                    device_peripherals = deviceInfo,
                    changedProperty = "DpiValueChanged"
                };
                OnNotify(_EventArgs);
            }
        }

        private void ILogicalDevice_TouchScrollSensitivityLevelChanged(ILogicalDevice3 arg1, int arg2)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                //Debug.WriteLine(arg2.ToString());
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
                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "TouchScrollSensitivityLevelChanged";
                    OnNotify(_EventArgs);
                }
                else
                {
                    writelog($"TouchScrollSensitivityLevelChanged: Error: deviceInfo is null");
                }
            }
        }

        private void ILogicalDevice_BackLightingControlsChanged(ILogicalDevice3 arg1, int arg2)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                //Debug.WriteLine(arg2.ToString());
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
                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "BackLightingControlsChanged";
                    OnNotify(_EventArgs);
                }
                else
                {
                    writelog($"BackLightingControlsChanged: Error: deviceInfo is null");
                }
            }
        }

        private void ILogicalDevice_BackLightingLevelChanged(ILogicalDevice3 arg1, int arg2)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                //Debug.WriteLine(arg2.ToString());
                writelog(arg2.ToString());
                if (deviceInfo != null)
                {
                    writelog($"BackLightingLevelChanged: value:{arg2}.....................{DateTime.Now:HH:mm:ss.ff}");
                    deviceInfo.BackLightingLevel = arg2;

                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "BackLightingLevelChanged";
                    OnNotify(_EventArgs);
                }
                else
                {
                    writelog($"BackLightingLevelChanged: Error: deviceInfo is null");
                }
            }
        }

        private void ILogicalDevice_BatteryStatusChanged(ILogicalDevice arg1, BatteryStatus arg2)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo != null)
                {
                    var oldStatus = deviceInfo.BatteryStatus;
                    deviceInfo.BatteryStatus = arg2.ToString();

                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "BatteryStatusChanged";
                    OnNotify(_EventArgs);
                    Debug.WriteLine($"BatteryStatusChanged: ID: {arg1.Id} Status: {arg2} Level: {deviceInfo.BatteryLevel}");
                    writelog($"BatteryStatusChanged: ID: {arg1.Id} Status: {arg2} Level: {deviceInfo.BatteryLevel}");

                    // << 250217 added by Hess to meet PIMS-341711
                    if (deviceInfo.BatteryStatus == "Charging" || oldStatus == "Charging")
                        LowBatteryIDs.Remove(deviceInfo.ID.ToString());
                    // >>
                    CheckLowBatteryOSD(deviceInfo);
                }
                else
                {
                    writelog($"BatteryStatusChanged: Error: deviceInfo is null");
                }
            }
        }

        //private void ILogicalDevice_BatteryStatusChanged(ILogicalDevice arg1, BatteryStatus arg2)
        //{
        //    if (_deviceHelper is { deviceInfo: not null })
        //    {
        //        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
        //        //Debug.WriteLine(arg2.ToString());
        //        writelog(arg2.ToString());
        //        if (deviceInfo != null)
        //        {
        //            deviceInfo.BatteryStatus = arg2.ToString();
        //            DeviceChangedEventArgs _EventArgs = new();
        //            _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
        //            _EventArgs.device_peripherals = deviceInfo;
        //            _EventArgs.changedProperty = "BatteryStatusChanged";
        //            OnNotify(_EventArgs);
        //            Debug.WriteLine($"BatteryStatusChanged: ID: {arg1.Id} Status: {arg2}");
        //            writelog($"BatteryStatusChanged: ID: {arg1.Id} Status: {arg2}");


        //            var settings = _DeviceManagerPlugin.GetGlobalSettingParam().Result;
        //            if (!settings.GlobalSetting_General.Low_Battery_Level)
        //                return;

        //            if (deviceInfo.BatteryLevel >= 0 && deviceInfo.BatteryLevel <= 9)
        //            {
        //                OSDType_Device type = OSDType_Device.Unknown;
        //                var deviceType = deviceInfo.LogicalDeviceType.ToUpper();
        //                if (deviceType.Contains("PEN"))
        //                {
        //                    if (deviceInfo.ModelNumber == "PN5122W" && deviceInfo.BatteryLevel > 6)
        //                    { return; }
        //                    type = OSDType_Device.Pen;
        //                }
        //                else if (deviceType.Contains("KEYBOARD"))
        //                {
        //                    type = OSDType_Device.Keyboard;
        //                }
        //                else if (deviceType.Contains("MOUSE"))
        //                {
        //                    type = OSDType_Device.Mouse;
        //                }
        //                else if (deviceType.Contains("HEADSET"))
        //                {
        //                    type = OSDType_Device.Headset;
        //                }
        //                _ = _DeviceManagerPlugin.ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.BatteryLow, type, deviceInfo.Name);
        //                Debug.WriteLine($"Show BatteryLow OSD: ID: {deviceInfo.ID} Level: {deviceInfo.BatteryLevel}");
        //                writelog($"Show BatteryLow OSD: ID: {deviceInfo.ID} Level: {deviceInfo.BatteryLevel}");
        //            }
        //        }
        //    }
        //}

        private Dictionary<string, OSDType_Device> LowBatteryIDs = new();
        private void CheckLowBatteryOSD(DeviceInfo deviceInfo)
        {
            try
            {
                if (_UserSettingsPlugin == null)
                    return;

                var settings = _UserSettingsPlugin.ReadGlobalSettings().Result;
                if (settings == null || settings.GlobalSetting_General == null)
                {
                    writelog("Retrieve global setting [Low_Battery_Level] got null data");
                    return;
                }
                if (!settings.GlobalSetting_General.Low_Battery_Level)
                {
                    writelog("Retrieve global setting [Low_Battery_Level] got disable result");
                    return;
                }

                if (deviceInfo.BatteryLevel >= 0 && deviceInfo.BatteryLevel <= 9 && !LowBatteryIDs.ContainsKey(deviceInfo.ID.ToString())) // && deviceInfo.BatteryStatus != "Charging")
                {
                    OSDType_Device type = OSDType_Device.Unknown;
                    var deviceType = deviceInfo.LogicalDeviceType.ToUpper();
                    var model = SAUICommonHelper.MappingModel(deviceInfo.ModelNumber);
                    var message = $"{deviceInfo.Name.Replace(deviceInfo.ModelNumber, "").Trim()} {model}";
                    if (deviceType.Contains("PEN"))
                    {
                        if (deviceInfo.ModelNumber == "PN5122W" && deviceInfo.BatteryLevel > 6)
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
                        //message = "Dell Headset ";
                    }
                    else if (SAUICommonHelper.EOLKBList.Contains(deviceInfo.ModelNumber))
                    {
                        type = OSDType_Device.Keyboard;
                        message = SAUICommonHelper.MappingEOLName(model);
                    }
                    else if (SAUICommonHelper.EOLMouseList.Contains(deviceInfo.ModelNumber))
                    {
                        type = OSDType_Device.Mouse;
                        message = SAUICommonHelper.MappingEOLName(model);
                    }

                    //_ = _DeviceManagerPlugin.ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.BatteryLow, type, deviceInfo.Name);
                    OSDEventArgs args = new()
                    {
                        Requester = "BatteryLow",
                        DeviceName = Screen.PrimaryScreen.DeviceName,
                        osd_type = OSDType.BatteryLow,
                        osd_device = type,
                        Guid = deviceInfo.ID,
                        Message = message
                    };
                    OnOSDNotify(args);
                    LowBatteryIDs.Add(deviceInfo.ID.ToString(), type);
                    Debug.WriteLine($"Show BatteryLow OSD: ID: {deviceInfo.ID} Level: {deviceInfo.BatteryLevel}");
                    writelog($"Show BatteryLow OSD: ID: {deviceInfo.ID} Level: {deviceInfo.BatteryLevel}");
                }
            }
            catch (Exception e)
            {
                writelog($"Retrieve global setting [Low_Battery_Level] to show osd with exception:{e.Message}");
            }
        }

        public Task UpdateLowBatteryOSD(bool showOSD)
        {
            writelog($"UpdateLowBatteryOSD: Value: {showOSD}");
            if (_deviceHelper is { deviceInfo: not null })
            {
                if (showOSD)
                {
                    foreach (var di in _deviceHelper.deviceInfo)
                    {
                        try
                        {
                            if (di.BatteryLevel >= 0 && di.BatteryLevel <= 9)
                            {
                                OSDType_Device type = OSDType_Device.Unknown;
                                var deviceType = di.LogicalDeviceType.ToUpper();
                                var model = SAUICommonHelper.MappingModel(di.ModelNumber);
                                var message = $"{di.Name.Replace(di.ModelNumber, "").Trim()} {model}";
                                if (deviceType.Contains("PEN"))
                                {
                                    if (di.ModelNumber == "PN5122W" && di.BatteryLevel > 6)
                                        continue;

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
                                else if (SAUICommonHelper.EOLKBList.Contains(di.ModelNumber))
                                {
                                    type = OSDType_Device.Keyboard;
                                    message = SAUICommonHelper.MappingEOLName(model);
                                }
                                else if (SAUICommonHelper.EOLMouseList.Contains(di.ModelNumber))
                                {
                                    type = OSDType_Device.Mouse;
                                    message = SAUICommonHelper.MappingEOLName(model);
                                }

                                OSDEventArgs args = new()
                                {
                                    Requester = "BatteryLow",
                                    DeviceName = Screen.PrimaryScreen.DeviceName,
                                    osd_type = OSDType.BatteryLow,
                                    osd_device = type,
                                    Guid = di.ID,
                                    Message = message
                                };
                                OnOSDNotify(args);
                                LowBatteryIDs.Add(di.ID.ToString(), type);
                                writelog($"Show BatteryLow OSD: ID: {di.ID} Level: {di.BatteryLevel}");
                            }
                        }
                        catch (Exception e)
                        {
                            writelog($"General setting Check [Low battery level] fail with exception:{e.Message}");
                        }
                    }
                }
                else
                {
                    foreach (var di in _deviceHelper.deviceInfo)
                    {
                        try
                        {
                            OSDEventArgs args = new()
                            {
                                Requester = "CloseBatteryLowOSD",
                                osd_type = OSDType.BatteryLow,
                                //osd_device = OSDType_Device.Keyboard,
                                Guid = di.ID
                            };
                            OnOSDNotify(args);
                            //args.osd_device = OSDType_Device.Mouse;
                            //OnOSDNotify(args);
                            //args.osd_device = OSDType_Device.Headset;
                            //OnOSDNotify(args);
                            //args.osd_device = OSDType_Device.Pen;
                            //OnOSDNotify(args);
                        }
                        catch (Exception e)
                        {
                            writelog($"General setting Uncheck [Low battery level] fail with exception:{e.Message}");
                        }
                    }
                    LowBatteryIDs.Clear();
                }
            }



            return Task.CompletedTask;
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
                    Debug.WriteLine($"BatteryLevelChanged: ID: {arg1.Id} Status: {deviceInfo.BatteryStatus} Level: {arg2}");
                    writelog($"BatteryLevelChanged: ID: {arg1.Id} Status: {deviceInfo.BatteryStatus} Level: {arg2}");

                    CheckLowBatteryOSD(deviceInfo);
                }
                else
                {
                    writelog($"BatteryLevelChanged: Error: deviceInfo is null");
                }
            }
        }

        private void ILogicalWiredAudio_MuteStatusChanged(ILogicalWiredAudio arg1, bool newMuteStatus)
        {
            //Console.WriteLine(newMuteStatus.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] ILogicalWiredAudio_MuteStatusChanged ... in " + newMuteStatus.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.MuteStatus = newMuteStatus;
                else
                {
                    writelog($"MuteStatusChanged: Error: deviceInfo is null");
                    return;
                }
                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "MuteStatusChanged";
                OnNotify(_EventArgs);
                //string osdinfo = string.Empty;
                //switch (deviceInfo.ModelNumber)
                //{
                //    //According to Figma string design. deviceInfo.Name
                //    case "SP3022":
                //        osdinfo = "Dell Speakerphone";
                //        break;
                //    case "SB522A":
                //        osdinfo = "Dell Soundbar";
                //        break;
                //}
                _logs.DebugMsg_1("[PeripheralsPlugin] ILogicalWiredAudio_MuteStatusChanged ... out " + newMuteStatus.ToString() + " , OSD in");
                try
                {
                    var settings = _UserSettingsPlugin.ReadGlobalSettings().Result;
                    if (settings == null || settings.GlobalSetting_General == null)
                    {
                        writelog("Retrieve global setting [Display_MuteState] got null data");
                        return;
                    }
                    if (!settings.GlobalSetting_General.Display_MuteState)
                    {
                        writelog("Retrieve global setting [Display_MuteState] got disable result");
                        return;
                    }
                    //if (_DeviceManagerPlugin.GetGlobalSettingParam().Result.GlobalSetting_General.Display_MuteState)
                    //{
                    //    Task.Run(async () => _DeviceManagerPlugin.ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.Mute, osdinfo, newMuteStatus));
                    //}
                    OSDType_Device type = OSDType_Device.Unknown;
                    var deviceType = deviceInfo.LogicalDeviceType.ToUpper();
                    var model = SAUICommonHelper.MappingModel(deviceInfo.ModelNumber);
                    var message = $"{deviceInfo.Name.Replace(deviceInfo.ModelNumber, "").Trim()} {model}";

                    OSDEventArgs args = new()
                    {
                        Requester = "Mute.Status",
                        DeviceName = Screen.PrimaryScreen.DeviceName,
                        osd_type = OSDType.Mute,
                        Message = message,
                        Status = newMuteStatus,
                        Guid = deviceInfo.ID
                    };
                    OnOSDNotify(args);
                    _logs.DebugMsg_1("[PeripheralsPlugin] ILogicalWiredAudio_MuteStatusChanged ... OSD out ");
                }
                catch (Exception e)
                {
                    writelog($"Retrieve global setting [Display_MuteState] to show osd with exception:{e.Message}");
                }
            }
        }

        private void _logicalDeviceHeadset_IsReadyChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_IsReadyChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.IsReady = newValue;
                else
                {
                    writelog($"IsReadyChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_IsDirtyChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.IsDirty = newValue;
                else
                {
                    writelog($"IsDirtyChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MicNoiseCancellationChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.MicNoiseCancellation = newValue;
                else
                {
                    writelog($"MicNoiseCancellationChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MicNCIncomingChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.MicNCIncoming = newValue;
                else
                {
                    writelog($"MicNCIncomingChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_BusyLightChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.BusyLight = newValue;
                else
                {
                    writelog($"BusyLightChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_VoiceGuidanceChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.VoiceGuidance = newValue;
                else
                {
                    writelog($"VoiceGuidanceChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MuteStatusChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.MuteStatus = newValue;
                else
                {
                    writelog($"[PeripheralsPlugin] MuteStatusChanged: Error: deviceInfo is null");
                    return;
                }

                DeviceChangedEventArgs _EventArgs = new();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = deviceInfo;
                _EventArgs.changedProperty = "MuteStatusChanged";
                OnNotify(_EventArgs);
                _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MuteStatusChanged after OnNotify ... ");
                try
                {
                    var settings = _UserSettingsPlugin.ReadGlobalSettings().Result;
                    if (settings == null || settings.GlobalSetting_General == null)
                    {
                        writelog("[PeripheralsPlugin] Retrieve global setting [Display_MuteState] got null data");
                        return;
                    }
                    if (!settings.GlobalSetting_General.Display_MuteState)
                    {
                        writelog("[PeripheralsPlugin] Retrieve global setting [Display_MuteState] got disable result");
                        return;
                    }
                    //if (_DeviceManagerPlugin.GetGlobalSettingParam().Result.GlobalSetting_General.Display_MuteState)
                    //{
                    //    Task.Run(async () => _ = _DeviceManagerPlugin.ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.Mute, deviceInfo.Name, newValue));
                    //}
                    OSDType_Device type = OSDType_Device.Unknown;
                    var deviceType = deviceInfo.LogicalDeviceType.ToUpper();
                    var model = SAUICommonHelper.MappingModel(deviceInfo.ModelNumber);
                    var message = $"{deviceInfo.Name.Replace(deviceInfo.ModelNumber, "").Trim()} {model}";
                    OSDEventArgs args = new()
                    {
                        Requester = "Mute.Status",
                        DeviceName = Screen.PrimaryScreen.DeviceName,
                        osd_type = OSDType.Mute,
                        Message = message,
                        Status = newValue,
                        Guid = deviceInfo.ID
                    };
                    OnOSDNotify(args);
                    _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_MuteStatusChanged OSD ... out ");
                }
                catch (Exception e)
                {
                    writelog($"Retrieve global setting [Display_MuteState] to show osd with exception:{e.Message}");
                }
            }
        }

        private void _logicalDeviceHeadset_SidetoneChanged(ILogicalDeviceHeadset logicalDeviceHeadset, bool newValue)
        {
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_SidetoneChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.Sidetone = newValue;
                else
                {
                    writelog($"SidetoneChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_SelectedPresetChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.SelectedPreset = newValue;
                else
                {
                    writelog($"SelectedPresetChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_SidetoneLevelChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.SidetoneLevel = newValue;
                else
                {
                    writelog($"SidetoneLevelChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_BandsGainChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.BandsGain = newValue;
                else
                {
                    writelog($"BandsGainChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_AncModeChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.AncMode = newValue;
                else
                {
                    writelog($"AncModeChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_AncGainChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.AncGain = newValue;
                else
                {
                    writelog($"AncGainChanged: Error: deviceInfo is null");
                    return;
                }

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
            //Console.WriteLine(newValue.ToString());
            _logs.DebugMsg_1("[PeripheralsPlugin] _logicalDeviceHeadset_WearDetectionChanged ... in " + newValue.ToString());
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDeviceHeadset.Id.ToString());
                if (deviceInfo != null)
                    deviceInfo.WearDetection = newValue;
                else
                {
                    writelog($"WearDetectionChanged: Error: deviceInfo is null");
                    return;
                }

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
            lock (_Lock)//this)
            {
                if (_isClientConnected && _iUpdateManager != null)
                {
                    _logs.DebugMsg_1($"[PeripheralsPlugin] IUpdateManager_IsAnyUpdateAvailableChanged start");
                    bool isDockHasUpdate = false;
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
                            string newModel = JudgmentList.ModelRename(updateItem.DeviceModelNumber);
                            if (!string.IsNullOrEmpty(newModel))
                            {
                                _logs.DebugMsg_1($"[PeripheralsPlugin] newModel = {newModel}");
                                _updateItems.DeviceModelNumber = newModel;
                            }
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

                            if (_updateItems.DeviceType.Equals(DeviceType.LogicalDock) ||
                                _updateItems.DeviceType.Equals(DeviceType.PhysicalWiredDock))
                            {
                                isDockHasUpdate = true;
                            }
                        }

                        if (_IsConnectingMultipleDocks && isDockHasUpdate)
                        {
                            _logs.DebugMsg_1($"[PeripheralsPlugin] _IsConnectingMultipleDocks = {_IsConnectingMultipleDocks} so remove dock update info in UpdateItems");
                            _updateHelper.UpdateItems.RemoveAll(x => x.DeviceType.Equals(DeviceType.LogicalDock) || x.DeviceType.Equals(DeviceType.PhysicalWiredDock));
                        }
                    }
                    else
                    {
                        _logs.DebugMsg_1($"[PeripheralsPlugin] _iUpdateManager.AllUpdateItems is null");
                    }

                    //Console.WriteLine(isAnyUpdateAvailable ? "UpdateAvailable" : "Already Updated.");
                    _logs.DebugMsg_1($"[PeripheralsPlugin] isAnyUpdateAvailable = {(isAnyUpdateAvailable ? "UpdateAvailable" : "Already Updated.")}");
                }
                //OnUpdateNotify(isAnyUpdateAvailable););//Bruce PIMS-346696 Updater event is moved to ScanDevices()
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
                else
                {
                    writelog($"DonglePairedDeviceCountChanged: Error: deviceInfo is null");
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

                deviceInfo.PairingStatusName = UpdateParingStausText((DonglePairingStatus)newPairingStatus);
                deviceInfo.Message = requestDeviceName;

                DeviceChangedEventArgs _EventArgs = new()
                {
                    type = DeviceChangedType.Peripherals_SettingsChange,
                    device_peripherals = deviceInfo,
                    changedProperty = $"DonglePairingStatusChanged"
                };
                Debug.WriteLine($"PairingStatusChanged: {deviceInfo.PairingStatusName}");
                writelog($"PairingStatusChanged: {deviceInfo.PairingStatusName}");
                OnNotify(_EventArgs);
            }
        }

        private void PhysicalAudioDeviceDongle_PairingStatusChanged(IPhysicalAudioDeviceDongle physicalDeviceDongle, AudioDonglePairingStatus newPairingStatus)
        {
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == physicalDeviceDongle.Id.ToString());
                // << 240712 fix empty dongle issue by Hess
                if (deviceInfo == null)
                    deviceInfo = new DeviceInfo();
                // >>

                deviceInfo.PairingStatusName = UpdateParingStausText((AudioDonglePairingStatus)newPairingStatus);
                deviceInfo.Message = "";

                DeviceChangedEventArgs _EventArgs = new()
                {
                    type = DeviceChangedType.Peripherals_SettingsChange,
                    device_peripherals = deviceInfo,
                    changedProperty = $"DonglePairingStatusChanged"
                };
                Debug.WriteLine($"AudioPairingStatusChanged: {deviceInfo.PairingStatusName}");
                writelog($"AudioPairingStatusChanged: {deviceInfo.PairingStatusName}");
                OnNotify(_EventArgs);
            }
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

        //private void IPhysicalDevicePen_IsdVersionChanged(IPhysicalPenDevice physicalPenDevice, string newValue)
        //{
        //    if (_deviceHelper is { deviceInfo: not null })
        //    {
        //        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == physicalPenDevice.Id.ToString());
        //        if (deviceInfo != null)
        //        {
        //            deviceInfo.IsdDriverVersion = newValue;

        //            DeviceChangedEventArgs _EventArgs = new();
        //            _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
        //            _EventArgs.device_peripherals = deviceInfo;
        //            _EventArgs.changedProperty = "PenVersionChanged";
        //            OnNotify(_EventArgs);
        //        }
        //        else
        //        {
        //            writelog($"PenVersionChanged: Error: deviceInfo is null");
        //        }
        //    }
        //}

        private void PhysicalDevicePen_IsdVersionChanged(IPhysicalPenDevice physicalPenDevice, string arg2, string arg3)
        {
            writelog($"IsdVersionChanged: physicalPenDeviceID: {physicalPenDevice.Id} Arg2: {arg2} Arg3: {arg3}");
            if (_deviceHelper is { deviceInfo: not null })
            {
                var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == physicalPenDevice.Id.ToString());
                if (deviceInfo != null)
                {
                    deviceInfo.IsdDriverVersion = arg2;
                    deviceInfo.IsdServiceVersion = arg3;

                    DeviceChangedEventArgs _EventArgs = new();
                    _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                    _EventArgs.device_peripherals = deviceInfo;
                    _EventArgs.changedProperty = "PenVersionChanged";
                    OnNotify(_EventArgs);
                }
                else
                {
                    writelog($"PenVersionChanged: Error: deviceInfo is null");
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
                else
                {
                    writelog($"IsMicEnumerationOnChanged: Error: deviceInfo is null");
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
                else
                {
                    writelog($"MousePrimaryButtonChanged: Error: deviceInfo is null");
                }
            }
        }

        public async Task<UpdateItemInfo> GetDPeMAssemblyUpdateInfo()
        {
            return await Task.Run(() =>
            {
                if (_updateItems == null)
                    return new UpdateItemInfo();
                //Console.WriteLine(Convert.ToString(_updateItems.NewVersion));
                writelog(Convert.ToString(_updateItems.NewVersion));
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
                    lock (_Lock)//this)
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
        public Task<int> GetIODongleCountGen3AgoCount()
        {
            return System.Threading.Tasks.Task.FromResult(_IODongleCount_Gen3Ago);
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

        private RegistryMonitor_Copilot registryMonitor_Copilot = null;
        public Task<bool> StartCopilotRegistryMonitor()
        {
            writelog("PeripheralPlugin StartCopilotRegistryMonitor requested ...");
            try
            {
                if (registryMonitor_Copilot == null)
                {
                    writelog("Monitor ICC change initiate...");
                    registryMonitor_Copilot = new RegistryMonitor_Copilot(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot");
                    registryMonitor_Copilot.RegChanged += new EventHandler(OnRegChanged_Copilot);
                    registryMonitor_Copilot.Start();
                    writelog("Monitor ICC change started");
                    return System.Threading.Tasks.Task.FromResult(true);
                }

                return System.Threading.Tasks.Task.FromResult(false);
            }
            catch (Exception ex)
            {
                writelog($"PeripheralPlugin StartCopilotRegistryMonitor Exception : {ex.Message} ...");
                return System.Threading.Tasks.Task.FromResult(false);
            }
        }
        private void OnRegChanged_Copilot(object sender, EventArgs e)
        {
            var di = new DeviceInfo();
            if (e == null)
                di.Message = "false";
            else
                di.Message = "true";

            DeviceChangedEventArgs _EventArgs = new()
            {
                type = DeviceChangedType.Peripherals_SettingsChange,
                device_peripherals = di,
                changedProperty = "CopilotEnableChanged"
            };
            OnNotify(_EventArgs);
        }
        private void OnError_Copilot(object sender, ErrorEventArgs e)
        {
            StopCopilotRegistryMonitor();
        }
        //public Task<bool> StopCopilotRegistryMonitor()
        //{
        //    writelog("PeripheralPlugin StopRegistryMonitor_ICC requested ...");

        //    if (registryMonitor_Copilot != null)
        //    {
        //        registryMonitor_Copilot.Stop();
        //        registryMonitor_Copilot.RegChanged -= new EventHandler(OnRegChanged_Copilot);
        //        registryMonitor_Copilot = null;
        //        return System.Threading.Tasks.Task.FromResult(true);
        //    }
        //    registryMonitor_Copilot = null;
        //    return System.Threading.Tasks.Task.FromResult(false);
        //}
        public async Task<bool> StopCopilotRegistryMonitor()
        {
            writelog("PeripheralPlugin StopRegistryMonitor_ICC requested ...");

            try
            {
                if (registryMonitor_Copilot != null)
                {
                    registryMonitor_Copilot.Stop();
                    registryMonitor_Copilot.RegChanged -= new EventHandler(OnRegChanged_Copilot);
                    registryMonitor_Copilot.Dispose();
                    return await Task.FromResult(true);
                }
            }
            catch (Exception ex)
            {
                writelog($"Error stopping registry monitor: {ex.Message}");
            }
            finally
            {
                registryMonitor_Copilot = null;
            }

            return await Task.FromResult(false);
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
            Debug.WriteLine(text);
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

        /*private void InitializeDeviceManagerPlugin()
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
        }*/

        public void UpdateDTPInstance(IDTPProxyPlugin DTPInstance)
        {
            if (_DTPProxyPlugin == null)
            {
                _DTPProxyPlugin = DTPInstance;
                writelog($"Assign instance {nameof(DTPInstance)} to DTH peripheral plugin");
            }
        }

        public void UpdateSettingsInstance(ISettingsManagerDev SettingsInstance)
        {
            if (_UserSettingsPlugin == null)
            {
                _UserSettingsPlugin = SettingsInstance;
                writelog($"Assign instance {nameof(SettingsInstance)} to DTH peripheral plugin");

                if (_deviceHelper != null && _deviceHelper.deviceInfo != null)
                {
                    foreach (var di in _deviceHelper.deviceInfo)
                    {
                        CheckLowBatteryOSD(di);
                    }
                }
            }
        }


        public event EventHandler<OSDEventArgs> Peripheral_OSD_Notify;

        private void OnOSDNotify(OSDEventArgs args)
        {
            EventHandler<OSDEventArgs> handler = Peripheral_OSD_Notify;
            Task.Run(() => handler?.Invoke(this, args));
            writelog($"Invoke Peripheral_OSD_Notify: {args.DeviceName}:{args.osd_type}:{args.osd_device}:{args.Message}");
        }
        private void CheckDocks()
        {
            //Scenario 1: If multiple docks are connected, and the message pops up while the dock is still connected
            //Scenario 2: If the first dock is connected, the message pops up
            writelog($"CheckDocks _IsConnectingMultipleDocks : {_IsConnectingMultipleDocks}");
            writelog($"CheckDocks _DockCount : {_DockCount}");
            if ((_IsConnectingMultipleDocks && _DockCount >= 1) ||
                _DockCount >= 2)
            {
                if (_deviceHelper == null || _deviceHelper.deviceInfo == null)
                    return;

                ToastContentBuilder toastContentBuilder = new ToastContentBuilder();
                toastContentBuilder.AddArgument(LangHelper.Instance["Warning"]);
                toastContentBuilder.AddText(LangHelper.Instance["Warning"]);
                toastContentBuilder.AddText(LangHelper.Instance["Multiple_docks_are_detected3"]);
                toastContentBuilder.Show(); // 顯示Toast通知
                if (!_IsConnectingMultipleDocks)
                {
                    DeviceInfo device = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhysicalDeviceType.Equals(DeviceType.LogicalDock) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWiredDock));
                    _IsConnectingMultipleDocks = true;
                    if (device != null)
                    {
                        device.IsConnected = false;

                        DeviceChangedEventArgs _EventArgs = new()
                        {
                            type = DeviceChangedType.Display_UnPlug,
                            device_peripherals = device,
                            changedProperty = "LogicalDeviceRemoved"
                        };
                        OnNotify(_EventArgs);
                    }
                }
                //Bruce 02/24 If multiple docks are docked consecutively, all docks will remove
                writelog("ChangeDock Connecting multiple docks so remove all dock");
                _deviceHelper.deviceInfo.RemoveAll(x => x.PhysicalDeviceType.Equals(DeviceType.LogicalDock) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWiredDock));
            }
        }

        #region IDisposableObservable Support

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Override for Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            writelog($"Dispose: {disposing}");
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;

                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}