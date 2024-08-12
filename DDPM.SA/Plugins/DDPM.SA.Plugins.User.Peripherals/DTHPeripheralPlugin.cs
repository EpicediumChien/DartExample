#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// DPeMPlugin.cs created on 10/4/2022T3:37 PM
//

#endregion

using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using DPeM.Interfaces;
using DPeMAgents.Common;
using DPeMPublic.Common.Enums;
using System.Reflection;
using IndiLogic.DPeM.Broker;

namespace Dell.TechHub.Peripheral.Plugins {
  [Plugin(IDs.DTH_PERIPHERAL_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
  [Descriptor(Description = pluginDescription)]
  [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
  [PublishedUnelevatedInterface(new[] { typeof(IDPeM) })]
  public class DTHPeripheralPlugin : BaseAgentPlugin, IDPeM {
    #region Properties and fields

    private const string pluginName = "DPeMPlugin";
    private const string pluginVersion = "1.0.0";
    private const string pluginDescription = "This plugin implements DPeM user agent Plugin.";
    private const string publisherCompany = "Dell Inc.";
    private const string publisherWebsite = "https://www.dell.com";
    private const string publisherSupport = "This plugin implements DPeM sub-agent Plugin.";

    private readonly IAgent _agent;
    public const string PluginLogId = "DPeM";

    private DeviceHelper _deviceHelper;
    private UpdateHelper _updateHelper;
    private ClientInfo _clientInfo;

    #endregion

    #region Private Members

    private IClient _iClient;
    private IDeviceManager _iDeviceManager;
    private IUpdateManager _iUpdateManager;

    public bool UpdateAvailable { get; set; }

    private UpdateItemInfo _updateItems = new();

    #endregion

    public IUpdateManager IUpdateManager => _iUpdateManager;

    public IDeviceManager IDeviceManager => _iDeviceManager;

    #region Constructor

    public DTHPeripheralPlugin(IAgent agent) : base(agent, PluginLogId) {
      _agent = agent;
      IndiLogic.DPeM.Broker.Client.StatusEvent += Client_StatusEvent;
      IndiLogic.DPeM.Broker.Client.Start();
    }

    #endregion

    #region IDPeM implementation

    public event EventHandler Notify;

    public event EventHandler UpdateNotify;

    public void NotifyNow() {
      OnNotify(EventArgs.Empty);
    }

    public void UpdateNotifyNow() {
      OnUpdateNotify(EventArgs.Empty);
    }

    public Task<PluginCondition> GetDPeMPluginConditionAsync() {
      throw new NotImplementedException();
    }

    public async Task<DeviceHelper> GetDevices() {
      if (_deviceHelper != null) {
        return await Task.Run(() => _deviceHelper);
      }
      return new DeviceHelper();
    }

    public async Task<ClientInfo> GetDPeMClientInfo() {
      if (_clientInfo != null) {
        return await Task.Run(() => _clientInfo);
      }
      return new ClientInfo();
    }

    public void SetDPILevel(int newDPILevel, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetDPILevel(newDPILevel + 1);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.DpiLevel = newDPILevel;
          }
        }
      }
    }

    public void SetDPIValue(int newDPIValue, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice2 _logicalDevice2) {
          _logicalDevice2.SetDPIValue(newDPIValue);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.DpiLevelValue = newDPIValue.ToString();
          }
        }
      }
    }

    public void SetPrimaryMouseButton(MouseButton newMouseButton, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetPrimaryMouseButton(newMouseButton);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.MousePrimaryButton = newMouseButton;
          }
        }
      }
    }

    public void SetTouchScrollSensitivityLevel(int newTouchScrollSensitivityLevel, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetTouchScrollSensitivityLevel(newTouchScrollSensitivityLevel);

          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.TouchScrollSensitivityLevel = newTouchScrollSensitivityLevel;
          }
        }
      }
    }

    public void SetCollaborationKeyEnable(bool newValue, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetCollaborationKeyEnable(newValue);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.IsCollaborationKeyEnable = newValue;
          }


        }
      }
    }

    public void SetCollaborationCameraEnable(bool newValue, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetCollaborationCameraEnable(newValue);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.IsCollaborationCameraEnable = newValue;
          }

        }
      }
    }

    public void SetCollaborationScreenShareEnable(bool newValue, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetCollaborationScreenShareEnable(newValue);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.IsCollaborationScreenShareEnable = newValue;
          }
        }
      }
    }

    public void SetCollaborationChatEnable(bool newValue, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetCollaborationChatEnable(newValue);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.IsCollaborationChatEnable = newValue;
          }
        }
      }
    }

    public void SetCollaborationMicEnable(bool newValue, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetCollaborationMicEnable(newValue);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.IsCollaborationMicEnable = newValue;
          }
        }
      }
    }

    public void SetCollaborationBlinkEffectEnable(bool newValue, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetCollaborationBlinkEffectEnable(newValue);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.IsCollaborationBlinkEffectEnable = newValue;
          }
        }
      }
    }

    public void SetCollaborationDoubleTapEnable(bool newValue, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetCollaborationDoubleTapEnable(newValue);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.IsCollaborationDoubleTapEnable = newValue;
          }
        }
      }
    }

    public void SetBackLightingControls(int newValue, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetBackLightingControls(newValue);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.BackLightingControls = newValue;
          }
        }
      }
    }

    public void SetBackLightingLevel(int newValue, Guid deviceId) {
      foreach (var device in _iDeviceManager.Devices) {
        var logicalDevice = device.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (logicalDevice is ILogicalDevice3 _logicalDevice3) {
          _logicalDevice3.SetBackLightingLevel(newValue);
          DeviceInfo _deviceInfo = _deviceHelper.deviceInfo.Where(x => x.ID == deviceId).FirstOrDefault();
          if (_deviceInfo != null) {
            _deviceInfo.BackLightingLevel = newValue;
          }
        }
      }
    }

    #endregion

    #region Overriding methods

    protected override void OnPluginStarting() {
      _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
      Log.Info("Enter DPeMTask1 ");
    }

    #endregion

    #region Private Methods

    private void ScanDevices() {
      _deviceHelper = new DeviceHelper {
        deviceInfo = new List<DeviceInfo>()
      };
      _deviceHelper.DPeMSDKVersion = Assembly.GetAssembly(typeof(ILogicalDevice)).GetName().Version.ToString();
      _deviceHelper.DCFVersion = Assembly.GetAssembly(typeof(PluginOrderGroupType)).GetName().Version.ToString();
      _deviceHelper.DPeMSubAgentVersion = Assembly.GetAssembly(typeof(DTHPeripheralPlugin)).GetName().Version.ToString();

      //_deviceHelper.DPeMSDKVersion = IndiLogic.DPeM.Broker.Assembly.GetName();
      foreach (var device in _iDeviceManager.Devices) {
        foreach (var item in device.Devices) {
          DeviceInfo info = new() {
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

          if (item.ParentPhysicalDevice.Type == DeviceType.PhysicalDongle) {
            if (item.ParentPhysicalDevice is IPhysicalDeviceDongle physicalDeviceDongle) {
              info.PairingStatusName = UpdateParingStausText(physicalDeviceDongle.PairingStatus);
              info.MaxPairingSlots = physicalDeviceDongle.MaxPairingSlots;
              info.PairedDeviceCount = physicalDeviceDongle.PairedDeviceCount;
              info.IsPhysicalDeviceDongle = true; //Because if it is a physical device dongle it will return true.
              physicalDeviceDongle.PairedDeviceCountChanged += IPhysicalDeviceDongle_PairedDeviceCountChanged;
              physicalDeviceDongle.PairingStatusChanged += IPhysicalDeviceDongle_PairingStatusChanged;
            }
          }

          if (item is ILogicalDevice2 _logicalDevice2) {
            info.IsCollabsKeysSupported = _logicalDevice2.IsCollabsKeysSupported;
            info.CollabsKeysSupported = _logicalDevice2.IsCollabsKeysSupported ? "Supported" : "Not Supported";
            info.DpiLevelValue = _logicalDevice2.DpiLevelValue.ToString();
            info.PairedHostName1 = _logicalDevice2.PairedHostName1;
            info.PairedHostName2 = _logicalDevice2.PairedHostName2;
            info.PairedHostName3 = _logicalDevice2.PairedHostName3;
            info.LogicalDeviceType = _logicalDevice2.Type.ToString();
            info.PhysicalDeviceType = device.Type.ToString();
            info.TotalNumberOfPairedHostName = _logicalDevice2.TotalNumberOfPaiedHostName;
            info.IsDPILevelSupported = _logicalDevice2.IsDPILevelSupported;

            _logicalDevice2.DpiLevelChanged += ILogicalDevice_DpiLevelChanged;
          }

          if (item is ILogicalDevice3 _logicalDevice3) {
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


            _logicalDevice3.MousePrimaryButtonChanged += ILogicalDevice_MousePrimaryButtonChanged;
            _logicalDevice3.DPIValueChanged += ILogicalDevice_DpiValueChanged;
            _logicalDevice3.TouchScrollSensitivityLevelChanged += ILogicalDevice_TouchScrollSensitivityLevelChanged;
            _logicalDevice3.BackLightingControlsChanged += ILogicalDevice_BackLightingControlsChanged;
            _logicalDevice3.BackLightingLevelChanged += ILogicalDevice_BackLightingLevelChanged;
          }

          if (item is ILogicalWiredAudio _logicalWiredAudio) {
            info.MuteStatus = _logicalWiredAudio.MuteStatus;
            _logicalWiredAudio.MuteStatusChanged += ILogicalWiredAudio_MuteStatusChanged;
          }

          _deviceHelper.deviceInfo.Add(info);

          //item.update
        }
        _iDeviceManager_DeviceAddedEvent(device);
      }

      Console.WriteLine(_deviceHelper.ToString());
    }

    private string UpdateParingStausText(DonglePairingStatus donglePairingStatus) {
      switch (donglePairingStatus) {
        case DonglePairingStatus.DonglePairingStatusStopped:
          return "Stopped";
        case DonglePairingStatus.DonglePairingStatusStarted:
          return "Stopped";
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

    #endregion

    #region EventHandlers

    private void OnNotify(EventArgs e) {
      if (Notify != null)
        Notify(this, e);
    }

    private void OnUpdateNotify(EventArgs e) {
      UpdateNotify?.Invoke(this, e);
    }

    private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e) {
      if (e?.ChangedPlugins == null || !e.ChangedPlugins.Any()) {
      }
    }

    private void Client_StatusEvent(ClientStatus status, IClient client) {
      if (status == ClientStatus.Connected) {
        _iClient = client;
        _iDeviceManager = _iClient.DeviceManager;
        _iDeviceManager.DeviceAddedEvent += _iDeviceManager_DeviceAddedEvent;
        _iDeviceManager.DeviceRemovedEvent += _iDeviceManager_DeviceRemovedEvent;
        ScanDevices();

        _iUpdateManager = _iClient.UpdateManager;
        _iUpdateManager.IsAnyUpdateAvailableChanged += IUpdateManager_IsAnyUpdateAvailableChanged;
      }
      else {
        _iDeviceManager = null;
        _iUpdateManager = null;
        _iClient = null;
      }

      //
      // Update UI with client and service status
      //
      _clientInfo = new ClientInfo {
        status = status.ToString()
      };

      if (status == ClientStatus.Connected) {
        if (_iClient != null) {
          _clientInfo.ServiceStatus = _iClient.ServiceStatus.ToString();
          _clientInfo.ApiVersion = _iClient.APIVersion.ToString();
        }
      }
      else {
        _clientInfo.ServiceStatus = "Unknown";
        _clientInfo.ApiVersion = "Unknown";
      }
      Console.WriteLine(_clientInfo.ToString());
    }

    private void _iDeviceManager_DeviceAddedEvent(IPhysicalDevice iPhysicalDevice) {
      System.Diagnostics.Debug.WriteLine("ParentPhysicalDevice Added, Id : " + iPhysicalDevice.Id + ", Name : " + iPhysicalDevice.Name);

      iPhysicalDevice.DeviceAddedEvent += IPhysicalDevice_DeviceAddedEvent;
      iPhysicalDevice.DeviceRemovedEvent += IPhysicalDevice_DeviceRemovedEvent;
    }

    private void _iDeviceManager_DeviceRemovedEvent(IPhysicalDevice iPhysicalDevice) {
      System.Diagnostics.Debug.WriteLine("ParentPhysicalDevice Removed, Id : " + iPhysicalDevice.Id + ", Name : " + iPhysicalDevice.Name);
    }

    private void IPhysicalDevice_DeviceAddedEvent(ILogicalDevice iLogicalDevice) {
      System.Diagnostics.Debug.WriteLine("LogicalDevice Added, Id : " + iLogicalDevice.Id + ", Name : " + iLogicalDevice.Name);
      iLogicalDevice.BatteryStatusChanged += ILogicalDevice_BatteryStatusChanged;

      ScanDevices();
      OnNotify(EventArgs.Empty);
    }

    private void ILogicalDevice_DpiLevelChanged(ILogicalDevice2 arg1, int arg2) {
      Console.WriteLine(arg2.ToString());

      if (_deviceHelper is { deviceInfo: not null }) {
        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
        Console.WriteLine(arg2.ToString());
        if (deviceInfo == null)
          return;
        if (arg2 == 0)
          return;
        deviceInfo.DpiLevel = arg2 - 1;
      }
      OnNotify(EventArgs.Empty);
    }

    private void ILogicalDevice_DpiValueChanged(ILogicalDevice3 arg1, int arg2) {
      Console.WriteLine(arg2.ToString());

      if (_deviceHelper is { deviceInfo: not null }) {
        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
        Console.WriteLine(arg2.ToString());
        if (deviceInfo != null) {
          deviceInfo.DpiLevelValue = arg2.ToString();
        }
      }
      OnNotify(EventArgs.Empty);
    }

    private void ILogicalDevice_TouchScrollSensitivityLevelChanged(ILogicalDevice3 arg1, int arg2) {
      Console.WriteLine(arg2.ToString());

      if (_deviceHelper is { deviceInfo: not null }) {
        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
        Console.WriteLine(arg2.ToString());
        if (deviceInfo != null) {
          deviceInfo.TouchScrollSensitivityLevel = arg2;
          deviceInfo.TouchSensitivityLevelValue = deviceInfo.TouchScrollSensitivityLevel switch {
            1 => 100,
            2 => 50,
            _ => 0
          };
          OnNotify(EventArgs.Empty);
        }
      }
      OnNotify(EventArgs.Empty);
    }

    private void ILogicalDevice_BackLightingControlsChanged(ILogicalDevice3 arg1, int arg2) {
      Console.WriteLine(arg2.ToString());

      if (_deviceHelper is { deviceInfo: not null }) {
        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
        Console.WriteLine(arg2.ToString());
        if (deviceInfo != null) {
          deviceInfo.BackLightingControls = arg2;
          deviceInfo.BackLightTabIndex = deviceInfo.BackLightingControls switch {
            1 => 0,
            3 => 2,
            6 => 1,
            _ => 0
          };
        }
      }
      OnNotify(EventArgs.Empty);
    }

    private void ILogicalDevice_BackLightingLevelChanged(ILogicalDevice3 arg1, int arg2) {
      Console.WriteLine(arg2.ToString());

      if (_deviceHelper is { deviceInfo: not null }) {
        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
        Console.WriteLine(arg2.ToString());
        if (deviceInfo != null) {
          deviceInfo.BackLightingLevel = arg2;
        }
      }
      OnNotify(EventArgs.Empty);
    }

    private void ILogicalDevice_BatteryStatusChanged(ILogicalDevice arg1, BatteryStatus arg2) {
      Console.WriteLine(arg2.ToString());

      if (_deviceHelper is { deviceInfo: not null }) {
        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
        Console.WriteLine(arg2.ToString());
        if (deviceInfo != null)
          deviceInfo.BatteryStatus = arg2.ToString();
      }
      OnNotify(EventArgs.Empty);
    }

    private void ILogicalWiredAudio_MuteStatusChanged(ILogicalWiredAudio arg1, bool newMuteStatus) {
      Console.WriteLine(newMuteStatus.ToString());

      if (_deviceHelper is { deviceInfo: not null }) {
        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == arg1.Id.ToString());
        if (deviceInfo != null)
          deviceInfo.MuteStatus = newMuteStatus;
      }
      OnNotify(EventArgs.Empty);
    }

    private void IPhysicalDevice_DeviceRemovedEvent(ILogicalDevice iLogicalDevice) {
      _deviceHelper.deviceInfo.Where(x => x.ID == iLogicalDevice.Id).ToList().ForEach(device => device.IsConnected = false);
      Console.WriteLine(_deviceHelper.ToString());
      OnNotify(EventArgs.Empty);
    }

    private void IUpdateManager_IsAnyUpdateAvailableChanged(bool isAnyUpdateAvailable) {
      _updateHelper = new UpdateHelper();
      UpdateAvailable = isAnyUpdateAvailable;
      _updateHelper.UpdateItems = new List<UpdateItemInfo>();
      foreach (var updateItem in _iUpdateManager.AllUpdateItems) {
        _updateItems = new UpdateItemInfo() { UpdateType = updateItem.Type.ToString(), UpdateSeverity = updateItem.Severity.ToString(), NewVersion = updateItem.NewVersion, Description = updateItem.Description };
        if (updateItem is IUpdateItem2 _updateItem2) {
          _updateItems.CurrentVersion = _updateItem2.CurrentVersion;
          _updateItems.DeviceId = _updateItem2.DeviceId;
          _updateItems.DeviceIndex = _updateItem2.DeviceIndex;
          _updateItems.DeviceModelNumber = _updateItem2.DeviceModelNumber;
          _updateItems.DeviceName = _updateItem2.DeviceName;
          _updateItems.DevicePath = _updateItem2.DevicePath;
          _updateItems.DeviceType = _updateItem2.DeviceType;
          _updateItems.FrimwareUpdatePath = _updateItem2.FrimwareUpdatePath;
          _updateItems.InstallPath = _updateItem2.InstallPath;
          _updateItems.InstanceId = _updateItem2.InstanceId;
          _updateItems.Priority = _updateItem2.Priority;
          _updateItems.ServerPath = _updateItem2.ServerPath;
          _updateItems.SupplierID = _updateItem2.SupplierID;
        }

        _updateHelper.UpdateItems.Add(_updateItems);
      }

      Console.WriteLine(isAnyUpdateAvailable ? "UpdateAvailable" : "Already Updated.");
      OnUpdateNotify(EventArgs.Empty);
    }

    private void IPhysicalDeviceDongle_PairedDeviceCountChanged(IPhysicalDeviceDongle physicalDeviceDongle, int newValue) {
      if (_deviceHelper is { deviceInfo: not null }) {
        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == physicalDeviceDongle.Id.ToString());
        Console.WriteLine(newValue.ToString());
        if (deviceInfo != null) {
          deviceInfo.PairedDeviceCount = newValue;
        }
      }
      OnNotify(EventArgs.Empty);
    }

    private void IPhysicalDeviceDongle_PairingStatusChanged(IPhysicalDeviceDongle physicalDeviceDongle, DonglePairingStatus newValue) {
      if (_deviceHelper is { deviceInfo: not null }) {
        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.PhyscialDeviceID.ToString() == physicalDeviceDongle.Id.ToString());
        Console.WriteLine(newValue.ToString());
        if (deviceInfo != null) {
          deviceInfo.PairingStatusName = UpdateParingStausText(newValue);
        }
      }
      OnNotify(EventArgs.Empty);
    }

    private void ILogicalDevice_MousePrimaryButtonChanged(ILogicalDevice3 logicalDevice3, MouseButton newValue) {
      Console.WriteLine(newValue.ToString());

      if (_deviceHelper is { deviceInfo: not null }) {
        var deviceInfo = _deviceHelper.deviceInfo.FirstOrDefault(x => x.ID.ToString() == logicalDevice3.Id.ToString());
        if (deviceInfo != null)
          deviceInfo.MousePrimaryButton = newValue;
      }
      OnNotify(EventArgs.Empty);
    }

    public async Task<UpdateItemInfo> GetDPeMAssemblyUpdateInfo() {
      return await Task.Run(() =>
      {
        if (_updateItems == null)
          return new UpdateItemInfo();
        Console.WriteLine(Convert.ToString(_updateItems.NewVersion));
        return _updateItems;
      });
    }

    public async Task<UpdateHelper> GetFWUpdateInfo() {
      if (_updateHelper != null) {
        return await Task.Run(() => _updateHelper);
      }
      return await Task.Run(() => new UpdateHelper());
    }

    public void DisplayNotification(string bannerInfo, string hyperlinkText, string bannerItemType) {
    }

    #endregion
  }
}