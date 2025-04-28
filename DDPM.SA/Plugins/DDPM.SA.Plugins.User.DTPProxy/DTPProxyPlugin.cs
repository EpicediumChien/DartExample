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
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Dell.TechHub.Commodity;
using Dell.TechHub.Sdk.Common.Identifiers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dell.TechHub.Commodity.Peripheral;
using Newtonsoft.Json.Linq;
using System.Text;
using DPeMPublic.Common.Enums;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using System.IO;
using Type = System.Type;
using DDPM.SA.Common.UI;
using System.Collections.Generic;
using static DDPM.RemoteManagement.Common.Interfaces.Params;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json;
using MS.WindowsAPICodePack.Internal;
using DDPM.SA.Common.Settings;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using Task = System.Threading.Tasks.Task;
using Microsoft;

namespace DDPM.SA.Plugins.User.DTPProxy
{
    [Plugin(DDPM.SA.Common.IDs.DDPM_DTP_Proxy_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [PluginRequires(Id = "{743D1C20-7A3E-4562-8D3E-C58F6ADFC050}", Version = "1.0.0", AllowDynamicResolving = true)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IDTPProxyPlugin) })]
    public class DTPProxyPlugin : BaseAgentPlugin, IDTPProxyPlugin, IDisposableObservable
    {
        private object _PeripheralLock = new object();

        #region Properties and fields

        private const string pluginName = "DTPProxyPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements DTP Proxy Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements DTP Proxy Plugin.";

        private IAgent _agent;
        private ICommodityClientSdk _commSdk;

        //private ClientAppId appId = new ClientAppId("{675f1370-b7ce-4113-8d6e-a128ee3bb74b}");
        private ClientAppId appId = new ClientAppId("{b397b9b3-04cb-4cdf-8a79-852d63cf4801}");

        private readonly object _PluginConditionLock = new object();


        private Type _globalperipheralInterfaceType;
        private Type _mouseInterfaceType;
        private Type _keyboardInterfaceType;
        private Type _penInterfaceType;
        private Type _speakerInterfaceType;
        private Type _dockInterfaceType;
        private Type _headsetInterfaceType;
        private Type _webcamInterfaceType;
        private Type _dongleInterfaceType;
        private Type _airaudioInterfaceType;
        private Type _rtkhubInterfaceType;

        private MethodInfo _globalperipheralMethodInfo;
        private MethodInfo _mouseMethodInfo;
        private MethodInfo _keyboardMethodInfo;
        private MethodInfo _penMethodInfo;
        private MethodInfo _speakerMethodInfo;
        private MethodInfo _dockMethodInfo;
        private MethodInfo _headsetMethodInfo;
        private MethodInfo _webcamMethodInfo;
        private MethodInfo _dongleMethodInfo;
        private MethodInfo _airaudioMethodInfo;
        private MethodInfo _rtkhubMethodInfo;

        private ItemId _itemID = null;
        private ICommodity _comdity;
        private const string GlobalPeripheralItemID = "DellPeripheral.GlobalPeripheral";
        private const string PenItemID = "DellPeripheral.Pen";
        private const string PenItemID0 = "DellPeripheral.Pen.0";
        private const string KeyboardItemID = "DellPeripheral.Keyboard";
        private const string KeyboardItemID0 = "DellPeripheral.Keyboard.0";
        private const string WebcamItemID = "DellPeripheral.Webcam";
        private const string HeadsetItemID = "DellPeripheral.Headset";
        private const string SpeakerItemID = "DellPeripheral.Speaker";
        private const string AirAudioItemID = "DellPeripheral.AirAudio";
        private const string RtkHubItemID = "DellPeripheral.RtkHub";
        private bool IsDTPReady = false;

        public const string PluginLogId = "DTPProxy";

        //Derek 1219 for webcam events handling
        private ICommodity _comdityWebcam = null;
        private List<WebcamEventHandleObject> webcamList = new List<WebcamEventHandleObject>();
        internal class WebcamEventHandleObject
        {
            public ICommodity webcamCommodity = null;
            public string webcamIndex = string.Empty; //DellPeripheral.Webcam.0
            public string DeviceName { get; set; } = string.Empty; //Dell Pro 24 Plus Video Conferencing Monitor
            public string DeviceId { get; set; } = string.Empty;     //36ce653b-7a0f-4c85-97f7-aad029cceeb2
            public string ModelNumber { get; set; } = string.Empty;  //P2424HEB
        };

        private ICommodity _comdityHeadset = null;
        private List<HeadsetEventHandleObject> headsetList = new List<HeadsetEventHandleObject>();
        internal class HeadsetEventHandleObject
        {
            public ICommodity headsetCommodity = null;
            public string headsetIndex = string.Empty;
            public string DeviceName { get; set; } = string.Empty;
            public string DeviceId { get; set; } = string.Empty;
            public string ModelNumber { get; set; } = string.Empty;
        };

        private ICommodity _comditySpeaker = null;
        private List<SpeakerEventHandleObject> speakerList = new List<SpeakerEventHandleObject>();
        internal class SpeakerEventHandleObject
        {
            public ICommodity speakerCommodity = null;
            public string speakerIndex = string.Empty;
            public string DeviceName { get; set; } = string.Empty;
            public string DeviceId { get; set; } = string.Empty;
            public string ModelNumber { get; set; } = string.Empty;
        };

        private ICommodity _comdityAirAudio = null;
        private List<AirAudioEventHandleObject> airaudioList = new List<AirAudioEventHandleObject>();
        internal class AirAudioEventHandleObject
        {
            public ICommodity airaudioCommodity = null;
            public string airaudioIndex = string.Empty;
            public string DeviceName { get; set; } = string.Empty;
            public string DeviceId { get; set; } = string.Empty;
            public string ModelNumber { get; set; } = string.Empty;
        };
#if Support_210
        private ICommodity _comdityRtkHub = null;
        private List<RtkHubEventHandleObject> rtkhubList = new List<RtkHubEventHandleObject>();
        internal class RtkHubEventHandleObject
        {
            public ICommodity rtkhubCommodity = null;
            public string rtkhubIndex = string.Empty;
            public string DeviceName { get; set; } = string.Empty;
            public string DeviceId { get; set; } = string.Empty;
            public string ModelNumber { get; set; } = string.Empty;
        };
#endif
        /// <summary>
        /// Webcam change event
        /// </summary>
        //public event EventHandler<bool>? Esi_IsCameraSensorCover_ChangeEvent;
        //public event EventHandler<int>? WALSnoozeTimeLeftInSeconds_ChangeEvent;
        //public event EventHandler<bool>? Esi_IsWALLockCountdownStartedChanged_ChangeEvent;
        //public event EventHandler<int>? Esi_WALLockCountdownChanged_ChangeEvent;


#endregion

        #region Constructor

        public DTPProxyPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;

            writelog("DTPProxyPlugin constructor ...");
            writelog($"Initializing the Commodity Client SDK...");
            var commodity = new ConnectedArgs("");

            InitializeDTPProxy();
            writelog($"DTPProxyPlugin constructor end...");
        }

        //Derek 1219
        ~DTPProxyPlugin()
        {
            _ = UnsubscribeDTPGlobalEventsAsync();
        }

        #endregion

        public event EventHandler<DeviceChangedEventArgs> Notify;

        public event EventHandler<bool> UpdateNotify;

        //Marked by Derek 1125 because they had covered by WebcamEventHandler
        //public event EventHandler<ZoomChangedArgs> ZoomChanged_Notify;
        //public event EventHandler<ZoomMeetingTypeChangedArgs> ZoomMeetingTypeChanged_Notify;
        //public event EventHandler<IsZoomMeetingActiveChangedArgs> IsZoomMeetingActive_Notify;
        //public event EventHandler<IsZoomScreenShareActiveChangedArgs> IsZoomScreenShareActive_Notify;

        //Derek 1120
        public event EventHandler<UpdateUINotify> DTPEventHandler;

        public event EventHandler<UpdateDTPProxyNotify> DTPProxyPluginSDKeventHandler;
        public void OnUIUpdateNotify(UpdateUINotify e)
        {
            _ = Task.Run(() => DTPEventHandler?.Invoke(this, e));
        }

        public event EventHandler<CMAIDEventArgs> CMAEventHandler;

        public void UpdateCMANotify(CMAIDEventArgs e)
        {
            _ = Task.Run(() => CMAEventHandler?.Invoke(this, e));
        }


        public void DTPProxyPluginSDKNotify(UpdateDTPProxyNotify e)
        {
            DTPProxyPluginSDKeventHandler?.Invoke(this, e);
        }

        public void NotifyNow()
        {
            OnNotify(new DeviceChangedEventArgs());
        }

        #region globalperipheral
        public async Task<bool> GetIsLockKeyNotificationsEnabledValue()
        {
            writelog($"Get IsLockKeyNotificationsEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (_globalperipheralMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_globalperipheralInterfaceType, commodity, "IsLockKeyNotificationsEnabled");
                    if (value is bool boolValue)
                    {
                        writelog($"Get IsLockKeyNotificationsEnabled Value: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"GetPropertyValue returned a value that is not of type bool or is null for IsLockKeyNotificationsEnabled");
                        return false;
                    }
                }
                else
                {
                    writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
                    return false;
                }
            }
            else
            {
                writelog($"[{nameof(GetIsLockKeyNotificationsEnabledValue)}] Could not retrieve the Commodity Interface for the item. _globalperipheralMethodInfo is null");
                return false;
            }
        }

        public async Task<bool> GetIsBatteryNotificationsEnabledValue()
        {
            writelog($"Get IsBatteryNotificationsEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (_globalperipheralMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_globalperipheralInterfaceType, commodity, "IsBatteryNotificationsEnabled");
                    if (value is bool boolValue)
                    {
                        writelog($"Get IsBatteryNotificationsEnabled Value: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"GetPropertyValue returned a value that is not of type bool or is null for IsBatteryNotificationsEnabled");
                        return false;
                    }
                }
                else
                {
                    writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
                    return false;
                }
            }
            else
            {
                writelog($"[{nameof(GetIsBatteryNotificationsEnabledValue)}]Could not retrieve the Commodity Interface for the  item. _globalperipheralMethodInfo is null");
                return false;
            }

        }

        public async Task<bool> GetIsPresenceDetectionSensnorStateNotificationsEnabledValue()
        {
            writelog($"Get IsPresenceDetectionSensnorStateNotificationsEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (_globalperipheralMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_globalperipheralInterfaceType, commodity, "IsPresenceDetectionSensnorStateNotificationsEnabled");
                    if (value is bool boolValue)
                    {
                        writelog($"Get IsPresenceDetectionSensnorStateNotificationsEnabled Value: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"GetPropertyValue returned a value that is not of type bool or is null for IsPresenceDetectionSensnorStateNotificationsEnabled");
                        return false;
                    }
                }
                else
                {
                    writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
                    return false;
                }
            }
            else
            {
                writelog($"[{nameof(GetIsPresenceDetectionSensnorStateNotificationsEnabledValue)}]Could not retrieve the Commodity Interface for the item. _globalperipheralMethodInfo is null");
                return false;
            }

        }

        public async Task<bool> GetIsAnalyticsEnabledValue()
        {
            writelog($"Get IsAnalyticsEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (_globalperipheralMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_globalperipheralInterfaceType, commodity, "IsAnalyticsEnabled");
                    if (value is bool boolValue)
                    {
                        writelog($"Get IsAnalyticsEnabled Value: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"GetPropertyValue returned a value that is not of type bool or is null for IsAnalyticsEnabled");
                        return false;
                    }
                }
                else
                {
                    writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
                    return false;
                }
            }
            else
            {
                writelog($"[{nameof(GetIsAnalyticsEnabledValue)}]Could not retrieve the Commodity Interface for the item. _globalperipheralMethodInfo is null");
                return false;
            }

        }

        public async Task<bool> GetIsQuickAccessMenuEnabledValue()
        {
            writelog($"Get IsQuickAccessMenuEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (_globalperipheralMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_globalperipheralInterfaceType, commodity, "IsQuickAccessMenuEnabled");
                    if (value is bool boolValue)
                    {
                        writelog($"Get IsQuickAccessMenuEnabled Value: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"GetPropertyValue returned a value that is not of type bool or is null for IsQuickAccessMenuEnabled");
                        return false;
                    }
                }
                else
                {
                    writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
                    return false;
                }
            }
            else
            {
                writelog($"[{nameof(GetIsQuickAccessMenuEnabledValue)}]Could not retrieve the Commodity Interface for the item. _globalperipheralMethodInfo is null");
                return false;
            }

        }

        public async Task<bool> GetIsMuteStatusNotificationsEnabledValue()
        {
            writelog($"Get IsMuteStatusNotificationsEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (_globalperipheralMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_globalperipheralInterfaceType, commodity, "IsMuteStatusNotificationsEnabled");
                    if (value is bool boolValue)
                    {
                        writelog($"Get IsMuteStatusNotificationsEnabled Value: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"GetPropertyValue returned a value that is not of type bool or is null for IsMuteStatusNotificationsEnabled");
                        return false;
                    }
                }
                else
                {
                    writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
                    return false;
                }
            }
            else
            {
                writelog($"[{nameof(GetIsMuteStatusNotificationsEnabledValue)}]Could not retrieve the Commodity Interface for the item. _globalperipheralMethodInfo is null");
                return false;
            }

        }

        public async Task<bool> GetIsQuickAccessMenuOSDEnabledValue()
        {
            writelog($"Get IsQuickAccessMenuOSDEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (_globalperipheralMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_globalperipheralInterfaceType, commodity, "IsQuickAccessMenuOSDEnabled");
                    if (value is bool boolValue)
                    {
                        writelog($"Get IsQuickAccessMenuOSDEnabled Value: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"GetPropertyValue returned a value that is not of type int or is null for IsQuickAccessMenuOSDEnabled");
                        return false;
                    }
                }
                else
                {
                    writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
                    return false;
                }
            }
            else
            {
                writelog($"[{nameof(GetIsQuickAccessMenuOSDEnabledValue)}]Could not retrieve the Commodity Interface for the item. _globalperipheralMethodInfo is null");
                return false;
            }

        }

        public async Task<bool> SetIsLockKeyNotificationsEnabledValue(bool newValue)
        {
            writelog($"Set IsLockKeyNotificationsEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
            {
                writelog($"SetPropertyValue IsLockKeyNotificationsEnabled :{newValue}");
                var result = SetPropertyValue(_globalperipheralInterfaceType, commodity, "IsLockKeyNotificationsEnabled", newValue);
                return result;
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
            }
            return false;
        }

        public async Task<bool> SetIsBatteryNotificationsEnabledValue(bool newValue)
        {
            writelog($"Set IsBatteryNotificationsEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
            {
                writelog($"SetPropertyValue IsBatteryNotificationsEnabled :{newValue}");
                var result = SetPropertyValue(_globalperipheralInterfaceType, commodity, "IsBatteryNotificationsEnabled", newValue);
                return result;
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
            }
            return false;
        }

        public async Task<bool> SetIsPresenceDetectionSensnorStateNotificationsEnabledValue(bool newValue)
        {
            writelog($"Set IsPresenceDetectionSensnorStateNotificationsEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
            {
                writelog($"SetPropertyValue IsPresenceDetectionSensnorStateNotificationsEnabled :{newValue}");
                var result = SetPropertyValue(_globalperipheralInterfaceType, commodity, "IsPresenceDetectionSensnorStateNotificationsEnabled", newValue);
                return result;
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
            }
            return false;
        }

        public async Task<bool> SetIsAnalyticsEnabledValue(bool newValue)
        {
            writelog($"Set IsAnalyticsEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
            {
                writelog($"SetPropertyValue IsAnalyticsEnabled :{newValue}");
                var result = SetPropertyValue(_globalperipheralInterfaceType, commodity, "IsAnalyticsEnabled", newValue);
                return result;
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
            }
            return false;
        }

        public async Task<bool> SetIsQuickAccessMenuEnabledValue(bool newValue)
        {
            writelog($"Set IsQuickAccessMenuEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
            {
                writelog($"SetPropertyValue IsQuickAccessMenuEnabled :{newValue}");
                var result = SetPropertyValue(_globalperipheralInterfaceType, commodity, "IsQuickAccessMenuEnabled", newValue);
                return result;
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
            }
            return false;
        }

        public async Task<bool> SetIsMuteStatusNotificationsEnabledValue(bool newValue)
        {
            writelog($"Set IsMuteStatusNotificationsEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
            {
                writelog($"SetPropertyValue IsMuteStatusNotificationsEnabled :{newValue}");
                var result = SetPropertyValue(_globalperipheralInterfaceType, commodity, "IsMuteStatusNotificationsEnabled", newValue);
                return result;
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
            }
            return false;
        }

        public async Task<bool> SetIsQuickAccessMenuOSDEnabledValue(bool newValue)
        {
            writelog($"Set IsQuickAccessMenuOSDEnabled Fun");
            _itemID = new ItemId(GlobalPeripheralItemID);
            if (await GetCommodityInterfaceInstanceAsync(_globalperipheralMethodInfo) is ICommodity commodity)
            {
                writelog($"SetPropertyValue IsQuickAccessMenuOSDEnabled :{newValue}");
                var result = SetPropertyValue(_globalperipheralInterfaceType, commodity, "IsQuickAccessMenuOSDEnabled", newValue);
                return result;
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_globalperipheralInterfaceType} for the item.");
            }
            return false;
        }

        private void _globalperipheralcom_IsQuickAccessMenuOSDEnabledChanged(object sender, IsQuickAccessMenuOSDEnabledChangedArgs e)
        {
            writelog($"[DTPProxy] IsQuickAccessMenuOSDEnabled {e.IsQuickAccessMenuOSDEnabled} Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
            SendDTPEventToUI(CreateEventMsg("GlobalPeripheralcom", "_globalperipheralcom_IsQuickAccessMenuOSDEnabledChanged", e.DeviceId));
        }

        private void _globalperipheralcom_IsQuickAccessMenuEnabledChanged(object sender, IsQuickAccessMenuEnabledChangedArgs e)
        {
            writelog($"[DTPProxy] IsQuickAccessMenuEnabled {e.IsQuickAccessMenuEnabled} Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
            SendDTPEventToUI(CreateEventMsg("GlobalPeripheralcom", "_globalperipheralcom_IsQuickAccessMenuEnabledChanged", e.DeviceId));
        }

        private void _globalperipheralcom_IsPresenceDetectionSensnorStateNotificationsEnabledChanged(object sender, IsPresenceDetectionSensnorStateNotificationsEnabledChangedArgs e)
        {
            writelog($"[DTPProxy] IsPresenceDetectionSensnorStateNotificationsEnabled {e.IsPresenceDetectionSensnorStateNotificationsEnabled} Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
            SendDTPEventToUI(CreateEventMsg("GlobalPeripheralcom", "_globalperipheralcom_IsPresenceDetectionSensnorStateNotificationsEnabledChanged", e.DeviceId));
        }

        private void _globalperipheralcom_IsMuteStatusNotificationsEnabledChanged(object sender, IsMuteStatusNotificationsEnabledChangedArgs e)
        {
            writelog($"[DTPProxy] IsMuteStatusNotificationsEnabled {e.IsMuteStatusNotificationsEnabled} Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
            SendDTPEventToUI(CreateEventMsg("GlobalPeripheralcom", "_globalperipheralcom_IsMuteStatusNotificationsEnabledChanged", e.DeviceId));
        }

        private void _globalperipheralcom_IsLockKeyNotificationsEnabledChanged(object sender, IsLockKeyNotificationsEnabledChangedArgs e)
        {
            writelog($"[DTPProxy] IsLockKeyNotificationsEnabled {e.IsLockKeyNotificationsEnabled} Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
            SendDTPEventToUI(CreateEventMsg("GlobalPeripheralcom", "_globalperipheralcom_IsLockKeyNotificationsEnabledChanged", e.DeviceId));
        }

        private void _globalperipheralcom_IsBatteryNotificationsEnabledChanged(object sender, IsBatteryNotificationsEnabledChangedArgs e)
        {
            writelog($"[DTPProxy] IsBatteryNotifications {e.IsBatteryNotificationsEnabled} Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
            SendDTPEventToUI(CreateEventMsg("GlobalPeripheralcom", "_globalperipheralcom_IsBatteryNotificationsEnabledChanged", e.DeviceId));
        }

        private void _globalperipheralcom_IsAnalyticsEnabledChanged(object sender, IsAnalyticsEnabledChangedArgs e)
        {
            writelog($"[DTPProxy] IsAnalyticsEnabled {e.IsAnalyticsEnabled} Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
            SendDTPEventToUI(CreateEventMsg("GlobalPeripheralcom", "_globalperipheralcom_IsAnalyticsEnabledChanged", e.DeviceId));
        }

        #endregion globalperipheral

        #region mouse
        public async Task<int> GetDpiValue(string Guid)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return -1; }

            if (_mouseMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_mouseInterfaceType, commodity, "DpiValue");
                    if (value is int intValue)
                    {
                        return intValue;
                    }
                    else
                    {
                        writelog($"GetPropertyValue returned a value that is not of type int or is null for DpiValue");
                        return -1;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
                    return -1;
                }
            }
            else
            {
                Debug.WriteLine($"[GetDpiValue]Could not retrieve the Commodity Interface for the {_itemID} item. _mouseMethodInfo is null");
                writelog($"[GetDpiValue]Could not retrieve the Commodity Interface for the {_itemID} item. _mouseMethodInfo is null");
                return -1;
            }

        }
        public async Task<JArray> GetMouseAssignableActions(string Guid)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return new JArray(); }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_mouseInterfaceType, commodity, "AssignableActions");
                if (value is JArray jArrayValue)
                {
                    return jArrayValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type JArray or is null for AssignableActions");
                    return new JArray();
                }
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return new JArray();
            }
        }
        public async Task<JArray> GetMouseAssignedActions(string Guid)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return new JArray(); }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_mouseInterfaceType, commodity, "AssignedActions");
                if (value is JArray jArrayValue)
                {
                    return jArrayValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type JArray or is null for AssignedActions");
                    return new JArray();
                }
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return new JArray();
            }
        }
        public async Task<JArray> GetMouseProgrammableKeys(string Guid)
        {
            Debug.Write($"GetMouseProgrammableKeys - Guid: {Guid}");
            if (!await GetItemIDAsync("Mouse", Guid))
            { return new JArray(); }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_mouseInterfaceType, commodity, "ProgrammableKeys");
                if (value is JArray jArrayValue)
                {
                    return jArrayValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type JArray or is null for ProgrammableKeys");
                    return new JArray();
                }
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return new JArray();
            }
        }
        public async Task<bool> DeleteMouseAllAssignedActions(string Guid)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_mouseInterfaceType, commodity, "DeleteAllAssignedActions");
                if (value is bool boolValue)
                {
                    return boolValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type bool or is null for DeleteAllAssignedActions");
                    return false;
                }
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return false;
            }
        }
        public async Task<JArray> GetAppSpecificProfiles(string Guid)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return new JArray(); }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_mouseInterfaceType, commodity, "AppSpecificProfiles");
                if (value is JArray jArrayValue)
                {
                    return jArrayValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type JArray or is null for AppSpecificProfiles");
                    return new JArray();
                }
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return new JArray();
            }
        }
        public async Task<string> GetMouseKeystrokeDisplayData(string Guid)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return string.Empty; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_mouseInterfaceType, commodity, "KeystrokeDisplayData");
                if (value is string stringValue)
                {
                    return stringValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type string or is null for KeystrokeDisplayData");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return string.Empty;
            }
        }
        public async Task<bool> StartMouseKeystrokeRecording(string Guid)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_mouseInterfaceType, commodity, "StartKeystrokeRecording");
                if (value is bool boolValue)
                {
                    return boolValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type bool or is null for StartKeystrokeRecording");
                    return false;
                }
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return false;
            }
        }
        public async Task<bool> StopMouseKeystrokeRecording(string Guid)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_mouseInterfaceType, commodity, "StopKeystrokeRecording");
                if (value is bool boolValue)
                {
                    return boolValue;
                }
                else
                {
                    Debug.WriteLine("GetPropertyValue returned a value that is not of type bool or is null for StopKeystrokeRecording");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return false;
            }
        }
        public async Task<int> GetTouchScrollSensitivityLevel(string Guid)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return -1; }

            if (_mouseMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_mouseInterfaceType, commodity, "TouchScrollSensitivityLevel");
                    if (value is int intValue)
                    {
                        return intValue;
                    }
                    else
                    {
                        writelog($"GetTouchScrollSensitivityLevel returned a value that is not of type int or is null for TouchScrollSensitivityLevel");
                        return -1;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
                    return -1;
                }
            }
            else
            {
                Debug.WriteLine($"[GetDpiValue]Could not retrieve the Commodity Interface for the {_itemID} item. _mouseMethodInfo is null");
                writelog($"[GetDpiValue]Could not retrieve the Commodity Interface for the {_itemID} item. _mouseMethodInfo is null");
                return -1;
            }

        }

        public async Task SetDpiValue(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_mouseInterfaceType, commodity, "DpiValue", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetMouseAction(string Guid, byte[] newValue)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_mouseInterfaceType, commodity, "AssignAction", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
            }
        }
        public async Task SetCurrentSelectedAppSpecificProfile(string Guid, string newValue)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_mouseInterfaceType, commodity, "CurrentSelectedAppSpecificProfile", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
            }
        }
        public async Task DeleteMouseAssignedAction(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_mouseInterfaceType, commodity, "DeleteAssignedAction", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
            }
        }
        public async Task SetMouseAssignDialogAction(string Guid, byte[] newValue)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_mouseInterfaceType, commodity, "AssignDialogAction", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
            }
        }
        public async Task SetMouseAssignKeystrokeAction(string Guid, byte[] newValue)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_mouseInterfaceType, commodity, "AssignKeystrokeAction", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
            }
        }
        public async Task<bool> RestoreToDefaultMouse(string Guid, bool isFromCli = true)
        {
            if (!IsDTPReady)
                return false;
            if (!await GetItemIDAsync("Mouse", Guid))
                return false;

            if (isFromCli)
            {
                string model;
                string profileID;
                if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_mouseInterfaceType, commodity, "ModelNumber");
                    model = value == null ? "" : (string)value;
                    if (model == "")
                        return false;

                    value = GetPropertyValue(_mouseInterfaceType, commodity, "CurrentSelectedAppSpecificProfile");
                    profileID = value == null ? "" : (string)value;
                    if (profileID == "")
                        return false;

                    var result = await DeleteMouseAllAssignedActions(Guid);
                    if (!result)
                        return false;

                    await SetCurrentSelectedAppSpecificProfile(Guid, "{76824745-CE06-4358-835D-7BB991CB71A0}"); //AllApp
                    await DeleteMouseAllAssignedActions(Guid);
                    await SetCurrentSelectedAppSpecificProfile(Guid, "{E0C9145B-BE8B-4423-B520-8CA71BE88E11}"); // Word
                    await DeleteMouseAllAssignedActions(Guid);
                    await SetCurrentSelectedAppSpecificProfile(Guid, "{37743697-4B39-45CD-B7F8-30027D1521ED}"); //Excel
                    await DeleteMouseAllAssignedActions(Guid);
                    await SetCurrentSelectedAppSpecificProfile(Guid, "{7BBECD91-F12A-4CC4-B005-526BA66BA657}"); //PowerPoint
                    await DeleteMouseAllAssignedActions(Guid);
                    await SetCurrentSelectedAppSpecificProfile(Guid, "{CCCE4E6F-C690-4EF5-BA19-F270C26C21B6}"); //Outlook
                    await DeleteMouseAllAssignedActions(Guid);
                    await SetCurrentSelectedAppSpecificProfile(Guid, profileID);

                    model = SAUICommonHelper.MappingModel(model);
                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\Actions\{model}.json");
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            if (DDPMFileSecurity.ValidateFilePath(filePath, out string info))
                                File.Delete(filePath);
                            else
                                writelog($"[DTPProxyPlugin][RestoreToDefaultPen][Path] Delete setting file failed: {info}");
                        }
                        catch (Exception ex)
                        {
                            writelog($"[DTPProxyPlugin][RestoreToDefaultPen] Delete setting file failed: {ex}");
                        }
                    }
                    var message = $"Mouse|RestoreToDefault|{Guid}|{model}";
                    SendDTPEventToUI(message);
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                    writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                    return false;
                }
            }
            else
            {
                var result = await DeleteMouseAllAssignedActions(Guid);
                return result;
            }
        }
        public async Task<bool> SetReportRate(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_mouseInterfaceType, commodity, "ReportRate", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return false;
            }
        }
        public async Task<bool> SetTouchScrollSensitivityLevel(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_mouseInterfaceType, commodity, "TouchScrollSensitivityLevel", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return false;
            }
        }

        #endregion

        #region keyboard

        //IKeyboardCommodity.DeleteAssignedAction

        public async Task DeleteKeyboardAssignedAction(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_keyboardInterfaceType, commodity, "DeleteAssignedAction", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
            }
        }

        //ProgrammableKeys
        public async Task<JArray> GetKbProgrammableKeys(string Guid)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return new JArray(); }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "ProgrammableKeys");
                if (value is JArray jArrayValue)
                {
                    return jArrayValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type JArray or is null for ProgrammableKeys");
                    return new JArray();
                }
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                return new JArray();
            }
        }
        public async Task<bool> DeleteKeyboardAllAssignedActions(string Guid)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "DeleteAllAssignedActions");
                if (value is bool boolValue)
                {
                    return boolValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type bool or is null for DeleteAllAssignedActions");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                return false;
            }
        }

        public async Task<JArray> GetKbAssignableActions(string Guid)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return new JArray(); }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "AssignableActions");
                if (value is JArray jArrayValue)
                {
                    return jArrayValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type JArray or is null for AssignableActions");
                    return new JArray();
                }
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_keyboardMethodInfo} for the {_itemID} item.");
                return new JArray();
            }
        }
        public async Task<JArray> GetKbAssignedActions(string Guid)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return new JArray(); }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "AssignedActions");
                if (value is JArray jArrayValue)
                {
                    return jArrayValue;
                }
                else
                {
                    Debug.WriteLine("GetPropertyValue returned a value that is not of type JArray or is null for AssignedActions");
                    return new JArray();
                }
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_keyboardMethodInfo} for the {_itemID} item.");
                return new JArray();
            }
        }

        public async Task<JArray> GetKeyboardDeviceItemsEx()
        {
            _itemID = new ItemId(KeyboardItemID);

            if (_keyboardMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_keyboardInterfaceType, commodity, "DeviceItemsEx");
                    if (value is JArray jArrayValue)
                    {
                        return jArrayValue;
                    }
                    else
                    {
                        writelog("GetPropertyValue returned a value that is not of type JArray or is null for DeviceItemsEx");
                        return new JArray();
                    }

                }
                else
                {
                    writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
                    return new JArray();
                }
            }
            else
            {
                writelog($"[GetDeviceItemsEx]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return new JArray();
            }

        }
        public async Task<string> GetKeyboardKeystrokeDisplayData(string Guid)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            {
                writelog($"GetKeyboardKeystrokeDisplayData canoot found: {Guid}");
                return string.Empty;
            }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "KeystrokeDisplayData");
                if (value is string stringValue)
                {
                    return stringValue;
                }
                else
                {
                    writelog("GetPropertyValue returned a value that is not of type string or is null for KeystrokeDisplayData");
                    return string.Empty;
                }
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                return string.Empty;
            }
        }

        public async Task<bool> StartKeyboardKeystrokeRecording(string Guid)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            {
                writelog($"Failed to get Item ID for Keyboard with Guid: {Guid}");
                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "StartKeystrokeRecording");
                if (value is bool boolValue)
                {
                    writelog($"StartKeystrokeRecording for Keyboard with Guid: {Guid} returned: {boolValue}");
                    return boolValue;
                }
                else
                {
                    writelog($"GetPropertyValue returned a value that is not of type bool or is null for StartKeystrokeRecording for Keyboard with Guid: {Guid}");
                    return false;
                }
            }
            else
            {
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                return false;
            }
        }

        public async Task<bool> StopKeyboardKeystrokeRecording(string Guid)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            {
                writelog($"Failed to get Item ID for Keyboard with Guid: {Guid}");
                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "StopKeystrokeRecording");
                if (value is bool boolValue)
                {
                    writelog($"StopKeystrokeRecording for Keyboard with Guid: {Guid} returned: {boolValue}");
                    return boolValue;
                }
                else
                {
                    writelog($"GetPropertyValue returned a value that is not of type bool or is null for StopKeystrokeRecording for Keyboard with Guid: {Guid}");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                return false;
            }
        }

        public async Task SetKbAssignKeystrokeAction(string Guid, byte[] newValue)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            {
                Debug.WriteLine($"Failed to get Item ID for Keyboard with Guid: {Guid}");
                writelog($"Failed to get Item ID for Keyboard with Guid: {Guid}");
                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                try
                {
                    SetPropertyValue(_keyboardInterfaceType, commodity, "AssignKeystrokeAction", newValue);
                    Debug.WriteLine($"Successfully set AssignKeystrokeAction for Keyboard with Guid: {Guid}");
                    writelog($"Successfully set AssignKeystrokeAction for Keyboard with Guid: {Guid}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting AssignKeystrokeAction for Keyboard with Guid: {Guid}. Exception: {ex.Message}");
                    writelog($"Error setting AssignKeystrokeAction for Keyboard with Guid: {Guid}. Exception: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"Could not SetPropertyValue the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                writelog($"Could not SetPropertyValue the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
            }
        }


        public async Task SetKbAssignDialogAction(string Guid, byte[] newValue)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            {
                Debug.WriteLine($"Failed to get Item ID for Keyboard with Guid: {Guid}");
                writelog($"Failed to get Item ID for Keyboard with Guid: {Guid}");
                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_keyboardInterfaceType, commodity, "AssignDialogAction", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not SetPropertyValue the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetKbAssignedAction(string Guid, byte[] newValue)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_keyboardInterfaceType, commodity, "AssignAction", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task<bool> RestoreToDefaultKB(string Guid)
        {
            if (!IsDTPReady)
                return false;
            if (!await GetItemIDAsync("Keyboard", Guid))
                return false;

            string model;
            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "ModelNumber");
                model = value == null ? "" : (string)value;
                if (model == "")
                    return false;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                return false;
            }

            var result = await DeleteKeyboardAllAssignedActions(Guid);
            if (!result)
                return false;

            model = SAUICommonHelper.MappingModel(model);
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\Actions\{model}.json");
            if (File.Exists(filePath))
            {
                try
                {
                    if (DDPMFileSecurity.ValidateFilePath(filePath, out string info))
                        File.Delete(filePath);
                    else
                        writelog($"[DTPProxyPlugin][RestoreToDefaultPen][Path] Delete setting file failed: {info}");
                }
                catch (Exception ex)
                {
                    writelog($"[DTPProxyPlugin][RestoreToDefaultPen] Delete setting file failed: {ex}");
                }
            }
            var message = $"Keyboard|RestoreToDefault|{Guid}|{model}";
            SendDTPEventToUI(message);
            return true;
        }

        #endregion

        #region Webcam

        //Derek 1120
        public async Task<JArray> GetWebcamDeviceItemsExAsync()
        {
            _itemID = new ItemId(WebcamItemID);

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "DeviceItemsEx");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for DeviceItemsEx.");
                        writelog("GetPropertyValue returned null for DeviceItemsEx.");
                        return new JArray();
                    }
                    else
                    {
                        Debug.WriteLine("GetPropertyValue successfully retrieved DeviceItemsEx.");
                        writelog("GetPropertyValue successfully retrieved DeviceItemsEx.");
                        return (JArray)value;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                    return new JArray();
                }
            }
            else
            {
                Debug.WriteLine($"[GetDeviceItemsEx]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetDeviceItemsEx]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");

                return new JArray();
            }
        }

        public Task<int> GetWebcamDeviceCountAsync()
        {
            try
            {
                return Task.FromResult(webcamList.Count);
            }
            catch (Exception e)
            {
                writelog($"GetWebcamDeviceCountAsync get exception {e.Message}");

                return Task.FromResult(0);
            }
        }

        //Derek 1221 for QAM
        public Task<string> GetWebcamDeviceID()
        {
            if (1 == webcamList.Count)
            {
                try
                {
                    //if (_comdityWebcam is IWebcamCommodity webcamCommodity)
                    //{
                    //    string jsonStr = webcamCommodity.DeviceItemsEx[0].ToString();
                    //    WebcamEventHandleObject jsonObject = JsonSerializer.Deserialize<WebcamEventHandleObject>(jsonStr)!;

                    //    writelog($"The only one webcam device's devcie id = {jsonObject.DeviceId}， and keey by SA's is {webcamList[0].DeviceId}");

                    //    return Task.FromResult(jsonObject.DeviceId);
                    //}
                    //else
                    //    return Task.FromResult(string.Empty);

                    return Task.FromResult(webcamList[0].DeviceId);
                }
                catch (Exception e)
                {
                    writelog($"GetWebcamDeviceID get exception {e.Message}");
                    return Task.FromResult(string.Empty);
                }
            }
            else
                return Task.FromResult(string.Empty);
        }

        public async Task<JArray> GetPresetProfiles(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return new JArray();
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "PresetProfiles");

                if (value == null)
                {
                    Debug.WriteLine("GetPropertyValue returned null for PresetProfiles.");
                    writelog("GetPropertyValue returned null for PresetProfiles.");
                    return new JArray();
                }
                else
                {
                    Debug.WriteLine("GetPropertyValue successfully retrieved PresetProfiles.");
                    writelog("GetPropertyValue successfully retrieved PresetProfiles.");
                    return (JArray)value;
                }
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return new JArray();
            }
        }
        public async Task<JArray> GetCustomProfiles(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return new JArray(); }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "CustomProfiles");

                if (value == null)
                {
                    Debug.WriteLine("GetPropertyValue returned null for CustomProfiles.");
                    writelog("GetPropertyValue returned null for CustomProfiles.");
                    return new JArray();
                }
                else
                {
                    Debug.WriteLine("GetPropertyValue successfully retrieved CustomProfiles.");
                    writelog("GetPropertyValue successfully retrieved CustomProfiles.");
                    return (JArray)value;
                }

            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return new JArray();
            }
        }
        public async Task<string> GetProfileName(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return ""; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "ProfileName");

                if (value == null)
                {
                    Debug.WriteLine("GetPropertyValue returned null for ProfileName.");
                    writelog("GetPropertyValue returned null for ProfileName.");
                    return "";
                }
                else
                {
                    Debug.WriteLine("GetPropertyValue successfully retrieved ProfileName.");
                    writelog("GetPropertyValue successfully retrieved ProfileName.");
                    return (string)value;
                }
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return "";
            }
        }
        public async Task<string> GetProfile(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return ""; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "Profile");

                if (value == null)
                {
                    Debug.WriteLine("GetPropertyValue returned null for Profile.");
                    writelog("GetPropertyValue returned null for Profile.");
                    return "";
                }
                else
                {
                    if (value is string stringValue)
                    {
                        Debug.WriteLine("GetPropertyValue successfully retrieved Profile.");
                        writelog("GetPropertyValue successfully retrieved Profile.");
                        return stringValue;
                    }
                    else
                    {
                        Debug.WriteLine("GetPropertyValue returned a non-string value for Profile.");
                        writelog("GetPropertyValue returned a non-string value for Profile.");
                        return "";
                    }
                }
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return "";
            }
        }
        public async Task<int> GetBrightness(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "Brightness");

                if (value == null)
                {
                    Debug.WriteLine("GetPropertyValue returned null for Brightness.");
                    writelog("GetPropertyValue returned null for Brightness.");
                    return -1;
                }
                else
                {
                    if (value is int intValue)
                    {
                        Debug.WriteLine("GetPropertyValue successfully retrieved Brightness.");
                        writelog("GetPropertyValue successfully retrieved Brightness.");
                        return intValue;
                    }
                    else
                    {
                        Debug.WriteLine("GetPropertyValue returned a non-integer value for Brightness.");
                        writelog("GetPropertyValue returned a non-integer value for Brightness.");
                        return -1;
                    }
                }
            }
            else
            {
                Debug.WriteLine($"[GetBrightnessValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"[GetBrightnessValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return -1;
            }
        }
        public async Task<string> GetCameraFirmwareVersion(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return string.Empty; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "FirmwareVersion");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for FirmwareVersion.");
                        writelog("GetPropertyValue returned null for FirmwareVersion.");
                        return "";
                    }
                    else
                    {
                        if (value is string stringValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved FirmwareVersion.");
                            writelog("GetPropertyValue successfully retrieved FirmwareVersion.");
                            return stringValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-string value for FirmwareVersion.");
                            writelog("GetPropertyValue returned a non-string value for FirmwareVersion.");
                            return "";
                        }
                    }

                }
                else
                {
                    Debug.WriteLine($"[GetCameraFirmwareVersion]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetCameraFirmwareVersion]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return string.Empty;
                }
            }
            else
            {
                Debug.WriteLine($"[GetCameraFirmwareVersion]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetCameraFirmwareVersion]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return string.Empty;
            }
        }
        public async Task<bool> GetIsWindowsHelloCapabilityVerified(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsWindowsHelloCapabilityVerified");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsWindowsHelloCapabilityVerified.");
                        writelog("GetPropertyValue returned null for IsWindowsHelloCapabilityVerified.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsWindowsHelloCapabilityVerified.");
                            writelog("GetPropertyValue successfully retrieved IsWindowsHelloCapabilityVerified.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsWindowsHelloCapabilityVerified.");
                            writelog("GetPropertyValue returned a non-boolean value for IsWindowsHelloCapabilityVerified.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[IsWindowsHelloCapabilityVerified]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[IsWindowsHelloCapabilityVerified]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[IsWindowsHelloCapabilityVerified]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[IsWindowsHelloCapabilityVerified]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }
        }
        public async Task<bool> GetIsAllSupportedResolutionsFound(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsAllSupportedResolutionsFound");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsAllSupportedResolutionsFound.");
                        writelog("GetPropertyValue returned null for IsAllSupportedResolutionsFound.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsAllSupportedResolutionsFound.");
                            writelog("GetPropertyValue successfully retrieved IsAllSupportedResolutionsFound.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsAllSupportedResolutionsFound.");
                            writelog("GetPropertyValue returned a non-boolean value for IsAllSupportedResolutionsFound.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[IsAllSupportedResolutionsFound]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[IsAllSupportedResolutionsFound]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[IsAllSupportedResolutionsFound]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[IsAllSupportedResolutionsFound]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }
        }
        public async Task<bool> GetIsPropertyFOVSupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPropertyFOVSupported");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsPropertyFOVSupported.");
                        writelog("GetPropertyValue returned null for IsPropertyFOVSupported.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsPropertyFOVSupported.");
                            writelog("GetPropertyValue successfully retrieved IsPropertyFOVSupported.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsPropertyFOVSupported.");
                            writelog("GetPropertyValue returned a non-boolean value for IsPropertyFOVSupported.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[CheckIsPropertyFOVSupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[CheckIsPropertyFOVSupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[CheckIsPropertyFOVSupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[CheckIsPropertyFOVSupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }
        }
        public async Task<int> GetFieldOfView(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "FieldOfView");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for FieldOfView.");
                        writelog("GetPropertyValue returned null for FieldOfView.");
                        return -1;
                    }
                    else
                    {
                        if (value is int intValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved FieldOfView.");
                            writelog("GetPropertyValue successfully retrieved FieldOfView.");
                            return intValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-integer value for FieldOfView.");
                            writelog("GetPropertyValue returned a non-integer value for FieldOfView.");
                            return -1;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetFieldOfViewValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetFieldOfViewValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return -1;
                }
            }
            else
            {
                Debug.WriteLine($"[GetFieldOfViewValue]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetFieldOfViewValue]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return -1;
            }
        }
        public async Task<bool> GetIsPropertyHDRSupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPropertyHDRSupported");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsPropertyHDRSupported.");
                        writelog("GetPropertyValue returned null for IsPropertyHDRSupported.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsPropertyHDRSupported.");
                            writelog("GetPropertyValue successfully retrieved IsPropertyHDRSupported.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsPropertyHDRSupported.");
                            writelog("GetPropertyValue returned a non-boolean value for IsPropertyHDRSupported.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[CheckIsPropertyHDRSupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[CheckIsPropertyHDRSupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[CheckIsPropertyHDRSupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[CheckIsPropertyHDRSupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }
        }
        public async Task<bool> GetIsHDROn(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsHDROn");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsHDROn.");
                        writelog("GetPropertyValue returned null for IsHDROn.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsHDROn.");
                            writelog("GetPropertyValue successfully retrieved IsHDROn.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsHDROn.");
                            writelog("GetPropertyValue returned a non-boolean value for IsHDROn.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetIsHDROn]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetIsHDROn]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsHDROnValue]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetIsHDROnValue]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }
        }

        // Content is same as GetIsPropertyHDRSupported() ?
        // Fixed by Elie. 2025/02/20
        public async Task<bool> GeIsPropertyAntiFlickerSupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPropertyAntiFlickerSupported");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsPropertyAntiFlickerSupported.");
                        writelog("GetPropertyValue returned null for IsPropertyAntiFlickerSupported.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsPropertyAntiFlickerSupported.");
                            writelog("GetPropertyValue successfully retrieved IsPropertyAntiFlickerSupported.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsPropertyAntiFlickerSupported.");
                            writelog("GetPropertyValue returned a non-boolean value for IsPropertyAntiFlickerSupported.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[IsPropertyAntiFlickerSupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[IsPropertyAntiFlickerSupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[IsPropertyAntiFlickerSupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[IsPropertyAntiFlickerSupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }
        }
        public async Task<int> GetAntiFlicker(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "AntiFlicker");

                if (value == null)
                {
                    Debug.WriteLine("GetPropertyValue returned null for AntiFlicker.");
                    writelog("GetPropertyValue returned null for AntiFlicker.");
                    return -1;
                }
                else
                {
                    if (value is int intValue)
                    {
                        Debug.WriteLine("GetPropertyValue successfully retrieved AntiFlicker.");
                        writelog("GetPropertyValue successfully retrieved AntiFlicker.");
                        return intValue;
                    }
                    else
                    {
                        Debug.WriteLine("GetPropertyValue returned a non-integer value for AntiFlicker.");
                        writelog("GetPropertyValue returned a non-integer value for AntiFlicker.");
                        return -1;
                    }
                }

            }
            else
            {
                Debug.WriteLine($"[GetAntiFlicker]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetAntiFlicker]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return -1;
            }
        }
        public async Task<bool> GetIsPropertyAutoFramingSupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPropertyAutoFramingSupported");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsPropertyAutoFramingSupported.");
                        writelog("GetPropertyValue returned null for IsPropertyAutoFramingSupported.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsPropertyAutoFramingSupported.");
                            writelog("GetPropertyValue successfully retrieved IsPropertyAutoFramingSupported.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsPropertyAutoFramingSupported.");
                            writelog("GetPropertyValue returned a non-boolean value for IsPropertyAutoFramingSupported.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[CheckIsPropertyAutoFramingSupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[CheckIsPropertyAutoFramingSupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[CheckIsPropertyAutoFramingSupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[CheckIsPropertyAutoFramingSupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }

        }
        public async Task<bool> GetIsESISupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsESISupported");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsESISupported.");
                        writelog("GetPropertyValue returned null for IsESISupported.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsESISupported.");
                            writelog("GetPropertyValue successfully retrieved IsESISupported.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsESISupported.");
                            writelog("GetPropertyValue returned a non-boolean value for IsESISupported.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[CheckIsESISupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[CheckIsESISupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[CheckIsESISupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[CheckIsESISupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }

        }
        public async Task<bool> GetIsAutoFramingOn(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsAutoFramingOn");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsAutoFramingOn.");
                        writelog("GetPropertyValue returned null for IsAutoFramingOn.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsAutoFramingOn.");
                            writelog("GetPropertyValue successfully retrieved IsAutoFramingOn.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsAutoFramingOn.");
                            writelog("GetPropertyValue returned a non-boolean value for IsAutoFramingOn.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetIsAutoFramingOnValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetIsAutoFramingOnValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsAutoFramingOnValue]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetIsAutoFramingOnValue]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }

        }
        public async Task<string> GetSupportedResolutions(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return string.Empty; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "SupportedResolutions");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for SupportedResolutions.");
                        writelog("GetPropertyValue returned null for SupportedResolutions.");
                        return string.Empty;
                    }
                    else
                    {
                        if (value is byte[] byteArray)
                        {
                            var str = Encoding.UTF8.GetString(byteArray);
                            Debug.WriteLine("GetPropertyValue successfully retrieved SupportedResolutions: " + str);
                            writelog("GetPropertyValue successfully retrieved SupportedResolutions: " + str);
                            return str;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-byte array value for SupportedResolutions.");
                            writelog("GetPropertyValue returned a non-byte array value for SupportedResolutions.");
                            return string.Empty;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetSupportedResolutions]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                    writelog($"[GetSupportedResolutions]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                    return string.Empty;
                }
            }
            else
            {
                Debug.WriteLine($"[GetSupportedResolutions]Could not retrieve the Commodity Interface for the {Guid} item. _webcamMethodInfo is null");
                writelog($"[GetSupportedResolutions]Could not retrieve the Commodity Interface for the {Guid} item. _webcamMethodInfo is null");
                return string.Empty;
            }

        }
        public async Task<string> GetSelectedResolution(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return string.Empty; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "SelectedResolution");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for SelectedResolution.");
                        writelog("GetPropertyValue returned null for SelectedResolution.");
                        return string.Empty;
                    }
                    else
                    {
                        if (value is byte[] byteArray)
                        {
                            var str = Encoding.UTF8.GetString(byteArray);
                            Debug.WriteLine("GetPropertyValue successfully retrieved SelectedResolution: " + str);
                            writelog("GetPropertyValue successfully retrieved SelectedResolution: " + str);
                            return str;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-byte array value for SelectedResolution.");
                            writelog("GetPropertyValue returned a non-byte array value for SelectedResolution.");
                            return string.Empty;
                        }
                    }

                }
                else
                {
                    Debug.WriteLine($"[GetSelectedResolution]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                    writelog($"[GetSelectedResolution]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                    return string.Empty;
                }
            }
            else
            {
                Debug.WriteLine($"[GetSelectedResolution]Could not retrieve the Commodity Interface for the {Guid} item. _webcamMethodInfo is null");
                writelog($"[GetSelectedResolution]Could not retrieve the Commodity Interface for the {Guid} item. _webcamMethodInfo is null");
                return string.Empty;
            }

        }
        public async Task<int> GetZoom(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "Zoom");

                if (value == null)
                {
                    Debug.WriteLine("GetPropertyValue returned null for Zoom.");
                    writelog("GetPropertyValue returned null for Zoom.");
                    return -1;
                }
                else
                {
                    if (value is int intValue)
                    {
                        Debug.WriteLine("GetPropertyValue successfully retrieved Zoom: " + intValue);
                        writelog("GetPropertyValue successfully retrieved Zoom: " + intValue);
                        return intValue;
                    }
                    else
                    {
                        Debug.WriteLine("GetPropertyValue returned a non-integer value for Zoom.");
                        writelog("GetPropertyValue returned a non-integer value for Zoom.");
                        return -1;
                    }
                }
            }
            else
            {
                Debug.WriteLine($"[GetZoom]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetZoom]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return -1;
            }
        }
        public async Task<int> GetFocus(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "Focus");

                if (value == null)
                {
                    Debug.WriteLine("GetPropertyValue returned null for Focus.");
                    writelog("GetPropertyValue returned null for Focus.");
                    return -1;
                }
                else
                {
                    if (value is int intValue)
                    {
                        Debug.WriteLine("GetPropertyValue successfully retrieved Focus: " + intValue);
                        writelog("GetPropertyValue successfully retrieved Focus: " + intValue);
                        return intValue;
                    }
                    else
                    {
                        Debug.WriteLine("GetPropertyValue returned a non-integer value for Focus.");
                        writelog("GetPropertyValue returned a non-integer value for Focus.");
                        return -1;
                    }
                }
            }
            else
            {
                Debug.WriteLine($"[GetFocus]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetFocus]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return -1;
            }
        }
        public async Task<bool?> GetIsFocusOn(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                Debug.WriteLine($"[GetIsFocusOn] Could not retrieve the Item ID for the {Guid} item.");
                writelog($"[GetIsFocusOn] Could not retrieve the Item ID for the {Guid} item.");
                return null;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsFocusOn");

                if (value == null)
                {
                    Debug.WriteLine($"[GetIsFocusOn] GetPropertyValue returned null for IsFocusOn.");
                    writelog($"[GetIsFocusOn] GetPropertyValue returned null for IsFocusOn.");
                    return null;
                }
                else if (value is bool boolValue)
                {
                    Debug.WriteLine($"[GetIsFocusOn] Successfully retrieved IsFocusOn: {boolValue}");
                    writelog($"[GetIsFocusOn] Successfully retrieved IsFocusOn: {boolValue}");
                    return boolValue;
                }
                else
                {
                    Debug.WriteLine($"[GetIsFocusOn] GetPropertyValue returned a non-boolean value for IsFocusOn.");
                    writelog($"[GetIsFocusOn] GetPropertyValue returned a non-boolean value for IsFocusOn.");
                    return null;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsFocusOn] Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetIsFocusOn] Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return null;
            }
        }

        public async Task<int> GetBgBlur(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "BgBlur");

                if (value == null)
                {
                    Debug.WriteLine("GetPropertyValue returned null for BgBlur.");
                    writelog("GetPropertyValue returned null for BgBlur.");
                    return -1;
                }
                else
                {
                    if (value is int intValue)
                    {
                        Debug.WriteLine("GetPropertyValue successfully retrieved BgBlur.");
                        writelog("GetPropertyValue successfully retrieved BgBlur.");
                        return intValue;
                    }
                    else
                    {
                        Debug.WriteLine("GetPropertyValue returned a non-integer value for BgBlur.");
                        writelog("GetPropertyValue returned a non-integer value for BgBlur.");
                        return -1;
                    }
                }

            }
            else
            {
                Debug.WriteLine($"[GetBgBlur]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetBgBlur]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return -1;
            }
        }
        public async Task<bool> GetIsPropertyBgBlurSupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPropertyBgBlurSupported");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsPropertyBgBlurSupported.");
                        writelog("GetPropertyValue returned null for IsPropertyBgBlurSupported.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsPropertyBgBlurSupported.");
                            writelog("GetPropertyValue successfully retrieved IsPropertyBgBlurSupported.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsPropertyBgBlurSupported.");
                            writelog("GetPropertyValue returned a non-boolean value for IsPropertyBgBlurSupported.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[CheckIsPropertyBgBlurSupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[CheckIsPropertyBgBlurSupported]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[CheckIsPropertyBgBlurSupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[CheckIsPropertyBgBlurSupported]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }

        }
        public async Task<bool> GetIsBgBlurEnable(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsBgBlurEnable");

                    if (value == null)
                    {
                        Debug.WriteLine("GetPropertyValue returned null for IsBgBlurEnable.");
                        writelog("GetPropertyValue returned null for IsBgBlurEnable.");
                        return false;
                    }
                    else
                    {
                        if (value is bool boolValue)
                        {
                            Debug.WriteLine("GetPropertyValue successfully retrieved IsBgBlurEnable.");
                            writelog("GetPropertyValue successfully retrieved IsBgBlurEnable.");
                            return boolValue;
                        }
                        else
                        {
                            Debug.WriteLine("GetPropertyValue returned a non-boolean value for IsBgBlurEnable.");
                            writelog("GetPropertyValue returned a non-boolean value for IsBgBlurEnable.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetIsBgBlurEnable]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetIsBgBlurEnable]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsBgBlurEnable]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetIsBgBlurEnable]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }

        }


        public async Task<int> GetPriority(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "Priority");

                if (value == null)
                {
                    Debug.WriteLine("[GetPriority] GetPropertyValue returned null for Priority.");
                    writelog("[GetPriority] GetPropertyValue returned null for Priority.");
                    return -1;
                }
                else if (value is int intValue)
                {
                    Debug.WriteLine($"[GetPriority] Successfully retrieved Priority: {intValue}");
                    writelog($"[GetPriority] Successfully retrieved Priority: {intValue}");
                    return intValue;
                }
                else
                {
                    Debug.WriteLine("[GetPriority] GetPropertyValue returned a non-integer value for Priority.");
                    writelog("[GetPriority] GetPropertyValue returned a non-integer value for Priority.");
                    return -1;
                }
            }
            else
            {
                Debug.WriteLine($"[GetPriority]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetPriority]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return -1;
            }
        }
        public async Task<bool?> GetIsAutoFramingTransitionOn(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                Debug.WriteLine($"[GetIsAutoFramingTransitionOn] Could not retrieve the Item ID for the {Guid} item.");
                writelog($"[GetIsAutoFramingTransitionOn] Could not retrieve the Item ID for the {Guid} item.");
                return null;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsAutoFramingTransitionOn");

                if (value == null)
                {
                    Debug.WriteLine($"[GetIsAutoFramingTransitionOn] GetPropertyValue returned null for IsAutoFramingTransitionOn.");
                    writelog($"[GetIsAutoFramingTransitionOn] GetPropertyValue returned null for IsAutoFramingTransitionOn.");
                    return null;
                }
                else if (value is bool boolValue)
                {
                    Debug.WriteLine($"[GetIsAutoFramingTransitionOn] Successfully retrieved IsAutoFramingTransitionOn: {boolValue}");
                    writelog($"[GetIsAutoFramingTransitionOn] Successfully retrieved IsAutoFramingTransitionOn: {boolValue}");
                    return boolValue;
                }
                else
                {
                    Debug.WriteLine($"[GetIsAutoFramingTransitionOn] GetPropertyValue returned a non-boolean value for IsAutoFramingTransitionOn.");
                    writelog($"[GetIsAutoFramingTransitionOn] GetPropertyValue returned a non-boolean value for IsAutoFramingTransitionOn.");
                    return null;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsAutoFramingTransitionOn] Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetIsAutoFramingTransitionOn] Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return null;
            }
        }

        public async Task<int> GetAutoFramingFrameSize(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "AutoFramingFrameSize");

                if (value == null)
                {
                    Debug.WriteLine("[GetAutoFramingFrameSize] GetPropertyValue returned null for AutoFramingFrameSize.");
                    writelog("[GetAutoFramingFrameSize] GetPropertyValue returned null for AutoFramingFrameSize.");
                    return -1;
                }
                else if (value is int intValue)
                {
                    Debug.WriteLine($"[GetAutoFramingFrameSize] Successfully retrieved AutoFramingFrameSize: {intValue}");
                    writelog($"[GetAutoFramingFrameSize] Successfully retrieved AutoFramingFrameSize: {intValue}");
                    return intValue;
                }
                else
                {
                    Debug.WriteLine("[GetAutoFramingFrameSize] GetPropertyValue returned a non-integer value for AutoFramingFrameSize.");
                    writelog("[GetAutoFramingFrameSize] GetPropertyValue returned a non-integer value for AutoFramingFrameSize.");
                    return -1;
                }
            }
            else
            {
                Debug.WriteLine($"[GetAutoFramingFrameSize]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetAutoFramingFrameSize]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return -1;
            }
        }
        public async Task<int> GetAutoFramingSensitivity(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "AutoFramingSensitivity");

                if (value == null)
                {
                    Debug.WriteLine("[GetAutoFramingSensitivity] GetPropertyValue returned null for AutoFramingSensitivity.");
                    writelog("[GetAutoFramingSensitivity] GetPropertyValue returned null for AutoFramingSensitivity.");
                    return -1;
                }
                else if (value is int intValue)
                {
                    Debug.WriteLine($"[GetAutoFramingSensitivity] Successfully retrieved AutoFramingSensitivity: {intValue}");
                    writelog($"[GetAutoFramingSensitivity] Successfully retrieved AutoFramingSensitivity: {intValue}");
                    return intValue;
                }
                else
                {
                    Debug.WriteLine("[GetAutoFramingSensitivity] GetPropertyValue returned a non-integer value for AutoFramingSensitivity.");
                    writelog("[GetAutoFramingSensitivity] GetPropertyValue returned a non-integer value for AutoFramingSensitivity.");
                    return -1;
                }
            }
            else
            {
                Debug.WriteLine($"[GetAutoFramingSensitivity]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetAutoFramingSensitivity]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return -1;
            }
        }
        public async Task<string> GetWebcamSerialNumber(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return ""; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "SerialNumber");

                if (value == null)
                {
                    Debug.WriteLine("[GetWebcamSerialNumber] GetPropertyValue returned null for SerialNumber.");
                    writelog("[GetWebcamSerialNumber] GetPropertyValue returned null for SerialNumber.");
                    return "";
                }
                else if (value is string stringValue)
                {
                    Debug.WriteLine($"[GetWebcamSerialNumber] Successfully retrieved SerialNumber: {stringValue}");
                    writelog($"[GetWebcamSerialNumber] Successfully retrieved SerialNumber: {stringValue}");
                    return stringValue;
                }
                else
                {
                    Debug.WriteLine("[GetWebcamSerialNumber] GetPropertyValue returned a non-string value for SerialNumber.");
                    writelog("[GetWebcamSerialNumber] GetPropertyValue returned a non-string value for SerialNumber.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetWebcamSerialNumber]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetWebcamSerialNumber]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return "";
            }
        }

        public async Task SetProfile(string Guid, string newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Profile", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetProfileName(string Guid, string newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "ProfileName", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task CreateCustomProfile(string Guid, string newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "CreateCustomProfile", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task DeleteProfile(string Guid, string newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "DeleteProfile", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task<bool> SetZoom(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "Zoom", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return false;
            }
        }
        public async Task<bool> SetAutoFramingSensitivity(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "AutoFramingSensitivity", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return false;
            }
        }
        public async Task<bool> SetAutoFramingFrameSize(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "AutoFramingFrameSize", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return false;
            }
        }
        public async Task<bool> SetIsAutoFramingOn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "IsAutoFramingOn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return false;
            }
        }
        public async Task<bool> SetIsAutoFramingTransitionOn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "IsAutoFramingTransitionOn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return false;
            }
        }
        public async Task<bool> SetFieldOfView(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "FieldOfView", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return false;
            }
        }
        public async Task<bool> SetIsFocusOn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "IsFocusOn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                return false;
            }
        }
        public async Task<bool> SetFocus(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "Focus", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                return false;
            }
        }
        public async Task<bool> SetPriority(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "Priority", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                return false;
            }
        }
        public async Task<bool> SetIsHDROn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "IsHDROn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return false;
            }
        }
        public async Task<bool> SetIsAutoWhiteBalanceOn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "IsAutoWhiteBalanceOn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                return false;
            }
        }
        public async Task<bool> SetAutoWhiteBalance(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "AutoWhiteBalance", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                return false;
            }
        }
        public async Task<bool> SetBrightness(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "Brightness", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                return false;
            }
        }
        public async Task<bool> SetSharpness(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "Sharpness", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                return false;
            }
        }
        public async Task<bool> SetContrast(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "Contrast", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                return false;
            }
        }
        public async Task<bool> SetSaturation(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "Saturation", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                return false;
            }
        }
        public async Task SetAntiFlicker(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "AntiFlicker", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetTilt(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Tilt", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetPan(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Pan", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetIsMicEnumerationOn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsMicEnumerationOn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetWALTime(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "WALTime", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetSnooze(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Snooze", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetSnoozeLength(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "SnoozeLength", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetIsProximitySensorEnable(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsProximitySensorEnable", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetIsWakeonApproachEnable(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsWakeonApproachEnable", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetIsWalkAwayLockEnable(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsWalkAwayLockEnable", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
            }
        }

        public async Task SetIsPrioritizeExternalWebcam(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsPrioritizeExternalWebcam", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
            }
        }

        public async Task ResetToDefault_webcam(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "ResetToDefault", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task<bool> SetBgBlur(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "BgBlur", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return false;
            }
        }

        public async Task<bool> SetIsBgBlurEnable(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");

                return false;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                return SetPropertyValue(_webcamInterfaceType, commodity, "IsBgBlurEnable", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                return false;
            }
        }
        public async Task<int> GetWALTime(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return -1;
            }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "WALTime");

                    if (value == null)
                    {
                        Debug.WriteLine("[GetWALTime] GetPropertyValue returned null for WALTime.");
                        writelog("[GetWALTime] GetPropertyValue returned null for WALTime.");
                        return -1;
                    }
                    else if (value is int intValue)
                    {
                        Debug.WriteLine($"[GetWALTime] Successfully retrieved WALTime: {intValue}");
                        writelog($"[GetWALTime] Successfully retrieved WALTime: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        Debug.WriteLine("[GetWALTime] GetPropertyValue returned a non-integer value for WALTime.");
                        writelog("[GetWALTime] GetPropertyValue returned a non-integer value for WALTime.");
                        return -1;
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetWALTime]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetWALTime]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return -1;
                }
            }
            else
            {
                Debug.WriteLine($"[GetWALTime]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetWALTime]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return -1;
            }
        }

        public async Task<int> GetSnooze(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                writelog($"GetItemIDAsync fail for webcam:{Guid}");
                return -1;
            }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "Snooze");

                    if (value == null)
                    {
                        Debug.WriteLine("[GetSnooze] GetPropertyValue returned null for Snooze.");
                        writelog("[GetSnooze] GetPropertyValue returned null for Snooze.");
                        return -1;
                    }
                    else if (value is int intValue)
                    {
                        Debug.WriteLine($"[GetSnooze] Successfully retrieved Snooze: {intValue}");
                        writelog($"[GetSnooze] Successfully retrieved Snooze: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        Debug.WriteLine("[GetSnooze] GetPropertyValue returned a non-integer value for Snooze.");
                        writelog("[GetSnooze] GetPropertyValue returned a non-integer value for Snooze.");
                        return -1;
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetSnooze]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetSnooze]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return -1;
                }
            }
            else
            {
                Debug.WriteLine($"[GetSnooze]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetSnooze]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return -1;
            }
        }

        public async Task<int> GetSnoozeLength(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "SnoozeLength");

                    if (value == null)
                    {
                        Debug.WriteLine("[GetSnoozeLength] GetPropertyValue returned null for SnoozeLength.");
                        writelog("[GetSnoozeLength] GetPropertyValue returned null for SnoozeLength.");
                        return -1;
                    }
                    else if (value is int intValue)
                    {
                        Debug.WriteLine($"[GetSnoozeLength] Successfully retrieved SnoozeLength: {intValue}");
                        writelog($"[GetSnoozeLength] Successfully retrieved SnoozeLength: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        Debug.WriteLine("[GetSnoozeLength] GetPropertyValue returned a non-integer value for SnoozeLength.");
                        writelog("[GetSnoozeLength] GetPropertyValue returned a non-integer value for SnoozeLength.");
                        return -1;
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetSnoozeLength]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetSnoozeLength]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return -1;
                }
            }
            else
            {
                Debug.WriteLine($"[GetSnoozeLength]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetSnoozeLength]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return -1;
            }

        }

        public async Task<bool> GetIsProximitySensorEnable(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsProximitySensorEnable");

                    if (value == null)
                    {
                        Debug.WriteLine("[GetIsProximitySensorEnable] GetPropertyValue returned null for IsProximitySensorEnable.");
                        writelog("[GetIsProximitySensorEnable] GetPropertyValue returned null for IsProximitySensorEnable.");
                        return false;
                    }
                    else if (value is bool boolValue)
                    {
                        Debug.WriteLine($"[GetIsProximitySensorEnable] Successfully retrieved IsProximitySensorEnable: {boolValue}");
                        writelog($"[GetIsProximitySensorEnable] Successfully retrieved IsProximitySensorEnable: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("[GetIsProximitySensorEnable] GetPropertyValue returned a non-boolean value for IsProximitySensorEnable.");
                        writelog("[GetIsProximitySensorEnable] GetPropertyValue returned a non-boolean value for IsProximitySensorEnable.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetIsProximitySensorEnable]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetIsProximitySensorEnable]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsProximitySensorEnable]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetIsProximitySensorEnable]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }

        }

        public async Task<bool> GetIsWakeonApproachEnable(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsWakeonApproachEnable");

                    if (value == null)
                    {
                        Debug.WriteLine("[GetIsWakeonApproachEnable] GetPropertyValue returned null for IsWakeonApproachEnable.");
                        writelog("[GetIsWakeonApproachEnable] GetPropertyValue returned null for IsWakeonApproachEnable.");
                        return false;
                    }
                    else if (value is bool boolValue)
                    {
                        Debug.WriteLine($"[GetIsWakeonApproachEnable] Successfully retrieved IsWakeonApproachEnable: {boolValue}");
                        writelog($"[GetIsWakeonApproachEnable] Successfully retrieved IsWakeonApproachEnable: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("[GetIsWakeonApproachEnable] GetPropertyValue returned a non-boolean value for IsWakeonApproachEnable.");
                        writelog("[GetIsWakeonApproachEnable] GetPropertyValue returned a non-boolean value for IsWakeonApproachEnable.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetIsWakeonApproachEnable]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetIsWakeonApproachEnable]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsWakeonApproachEnable]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetIsWakeonApproachEnable]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }

        }

        public async Task<bool> GetIsWalkAwayLockEnable(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsWalkAwayLockEnable");

                    if (value == null)
                    {
                        Debug.WriteLine("[GetIsWalkAwayLockEnable] GetPropertyValue returned null for IsWalkAwayLockEnable.");
                        writelog("[GetIsWalkAwayLockEnable] GetPropertyValue returned null for IsWalkAwayLockEnable.");
                        return false;
                    }
                    else if (value is bool boolValue)
                    {
                        Debug.WriteLine($"[GetIsWalkAwayLockEnable] Successfully retrieved IsWalkAwayLockEnable: {boolValue}");
                        writelog($"[GetIsWalkAwayLockEnable] Successfully retrieved IsWalkAwayLockEnable: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("[GetIsWalkAwayLockEnable] GetPropertyValue returned a non-boolean value for IsWalkAwayLockEnable.");
                        writelog("[GetIsWalkAwayLockEnable] GetPropertyValue returned a non-boolean value for IsWalkAwayLockEnable.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"[GetIsWalkAwayLockEnable]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetIsWalkAwayLockEnable]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsWalkAwayLockEnable]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetIsWalkAwayLockEnable]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }

        }
        public async Task<bool?> GetIsPrioritizeExternalWebcam(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                Debug.WriteLine("[GetIsPrioritizeExternalWebcam] Failed to get item ID for Webcam.");
                writelog("[GetIsPrioritizeExternalWebcam] Failed to get item ID for Webcam.");
                return null;
            }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPrioritizeExternalWebcam");

                if (value == null)
                {
                    Debug.WriteLine("[GetIsPrioritizeExternalWebcam] GetPropertyValue returned null for IsPrioritizeExternalWebcam.");
                    writelog("[GetIsPrioritizeExternalWebcam] GetPropertyValue returned null for IsPrioritizeExternalWebcam.");
                    return null;
                }
                else if (value is bool boolValue)
                {
                    Debug.WriteLine($"[GetIsPrioritizeExternalWebcam] Successfully retrieved IsPrioritizeExternalWebcam: {boolValue}");
                    writelog($"[GetIsPrioritizeExternalWebcam] Successfully retrieved IsPrioritizeExternalWebcam: {boolValue}");
                    return boolValue;
                }
                else
                {
                    Debug.WriteLine("[GetIsPrioritizeExternalWebcam] GetPropertyValue returned a non-boolean value for IsPrioritizeExternalWebcam.");
                    writelog("[GetIsPrioritizeExternalWebcam] GetPropertyValue returned a non-boolean value for IsPrioritizeExternalWebcam.");
                    return null;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsPrioritizeExternalWebcam] Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"[GetIsPrioritizeExternalWebcam] Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return null;
            }
        }


        public async Task<bool> GetIsZoomMeetingActive()
        {
            //_itemID = new ItemId("DellPeripheral.Webcam");

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsZoomMeetingActive");

                    if (value == null)
                    {
                        Debug.WriteLine("[GetIsZoomMeetingActive] GetPropertyValue returned null for IsZoomMeetingActive.");
                        writelog("[GetIsZoomMeetingActive] GetPropertyValue returned null for IsZoomMeetingActive.");
                        return false;
                    }
                    else if (value is bool boolValue)
                    {
                        Debug.WriteLine($"[GetIsZoomMeetingActive] Successfully retrieved IsZoomMeetingActive: {boolValue}");
                        writelog($"[GetIsZoomMeetingActive] Successfully retrieved IsZoomMeetingActive: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("[GetIsZoomMeetingActive] GetPropertyValue returned a non-boolean value for IsZoomMeetingActive.");
                        writelog("[GetIsZoomMeetingActive] GetPropertyValue returned a non-boolean value for IsZoomMeetingActive.");
                        return false;
                    }
                }
                else
                {
                    writelog($"[GetIsZoomMeetingActive]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                writelog($"[GetIsZoomMeetingActive]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }
        }

        public async Task<bool> GetIsZoomMeetingActive(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsZoomMeetingActive");

                    if (value == null)
                    {
                        Debug.WriteLine("[GetIsZoomMeetingActive] GetPropertyValue returned null for IsZoomMeetingActive.");
                        writelog("[GetIsZoomMeetingActive] GetPropertyValue returned null for IsZoomMeetingActive.");
                        return false;
                    }
                    else if (value is bool boolValue)
                    {
                        Debug.WriteLine($"[GetIsZoomMeetingActive] Successfully retrieved IsZoomMeetingActive: {boolValue}");
                        writelog($"[GetIsZoomMeetingActive] Successfully retrieved IsZoomMeetingActive: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("[GetIsZoomMeetingActive] GetPropertyValue returned a non-boolean value for IsZoomMeetingActive.");
                        writelog("[GetIsZoomMeetingActive] GetPropertyValue returned a non-boolean value for IsZoomMeetingActive.");
                        return false;
                    }
                }
                else
                {
                    writelog($"[GetIsZoomMeetingActive]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                writelog($"[GetIsZoomMeetingActive]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }

        }

        //Derek 1209 ZoomMeetingType should not return bool value
        //public async Task<bool> GetZoomMeetingType(string Guid)
        public async Task<int> GetZoomMeetingTypeAsync(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            {
                //return false; 

                return 4;
            }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "ZoomMeetingType");

                    if (value == null)
                    {
                        Debug.WriteLine("[GetZoomMeetingType] GetPropertyValue returned null for ZoomMeetingType.");
                        writelog("[GetZoomMeetingType] GetPropertyValue returned null for ZoomMeetingType.");
                        return -1;
                    }
                    else if (value is int intValue)
                    {
                        Debug.WriteLine($"[GetZoomMeetingType] Successfully retrieved ZoomMeetingType: {intValue}");
                        writelog($"[GetZoomMeetingType] Successfully retrieved ZoomMeetingType: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        Debug.WriteLine("[GetZoomMeetingType] GetPropertyValue returned a non-integer value for ZoomMeetingType.");
                        writelog("[GetZoomMeetingType] GetPropertyValue returned a non-integer value for ZoomMeetingType.");
                        return -1;
                    }
                }
                else
                {
                    writelog($"[GetZoomMeetingType]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");

                    //return false;
                    return 4;
                }
            }
            else
            {
                writelog($"[GetZoomMeetingType]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                //return false;

                return 4;
            }

        }
        public async Task<bool> GetIsZoomScreenShareActive(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsZoomScreenShareActive");

                    if (value == null)
                    {
                        Debug.WriteLine("[GetIsZoomScreenShareActive] GetPropertyValue returned null for IsZoomScreenShareActive.");
                        writelog("[GetIsZoomScreenShareActive] GetPropertyValue returned null for IsZoomScreenShareActive.");
                        return false;
                    }
                    else if (value is bool boolValue)
                    {
                        Debug.WriteLine($"[GetIsZoomScreenShareActive] Successfully retrieved IsZoomScreenShareActive: {boolValue}");
                        writelog($"[GetIsZoomScreenShareActive] Successfully retrieved IsZoomScreenShareActive: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("[GetIsZoomScreenShareActive] GetPropertyValue returned a non-boolean value for IsZoomScreenShareActive.");
                        writelog("[GetIsZoomScreenShareActive] GetPropertyValue returned a non-boolean value for IsZoomScreenShareActive.");
                        return false;
                    }
                }
                else
                {
                    writelog($"[GetIsZoomScreenShareActive]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                writelog($"[GetIsZoomScreenShareActive]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
            }

        }
        private async Task<bool> GetItemIDAsync(string type, string guid)
        {
            if (string.IsNullOrEmpty(type))
            {
                writelog($"Type is empty!");
                return false;
            }
            if (string.IsNullOrEmpty(guid))
            {
                writelog($"Guid is empty!");
                return false;
            }

            MethodInfo methodInfo = type switch
            {
                "GlobalPeripheral" => _globalperipheralMethodInfo,
                "Mouse" => _mouseMethodInfo,
                "Keyboard" => _keyboardMethodInfo,
                "Pen" => _penMethodInfo,
                "Webcam" => _webcamMethodInfo,
                "Headset" => _headsetMethodInfo,
                "Speaker" => _speakerMethodInfo,
                "Dongle" => _dongleMethodInfo,
                "Dock" => _dockMethodInfo,
                "AirAudio" => _airaudioMethodInfo,
                _ => null
            };
            Type interfaceType = type switch
            {
                "GlobalPeripheral" => _globalperipheralInterfaceType,
                "Mouse" => _mouseInterfaceType,
                "Keyboard" => _keyboardInterfaceType,
                "Pen" => _penInterfaceType,
                "Webcam" => _webcamInterfaceType,
                "Headset" => _headsetInterfaceType,
                "Speaker" => _speakerInterfaceType,
                "Dongle" => _dongleInterfaceType,
                "Dock" => _dockInterfaceType,
                "AirAudio" => _airaudioInterfaceType,
                _ => null
            };


            if (methodInfo == null)
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface to get Guid");
                writelog($"Could not retrieve the Commodity methodInfo to get Guid");
                return false;
            }

            if (interfaceType == null)
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface to get Guid");
                writelog($"Could not retrieve the Commodity interfaceType to get Guid");
                return false;
            }

            int i = 0;
            while (true)
            {
                _itemID = new ItemId($"DellPeripheral.{type}.{i}");
                writelog($"Checking ItemId: {_itemID}");

                if (await GetCommodityInterfaceInstanceAsync(methodInfo) is ICommodity commodity)
                {
                    writelog($"Commodity type: {commodity.GetType()}");
                    var value = GetPropertyValue(interfaceType, commodity, "DeviceId");

                    if (value == null)
                    {
                        writelog($"Not found {type} GUID: {guid}");
                        return false;
                    }

                    if (value is string deviceId)
                    {
                        Debug.WriteLine($"DeviceId: {deviceId}");
                        if (deviceId == guid)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"DeviceId is not a string for {type} GUID: {guid}");
                        writelog($"DeviceId is not a string for {type} GUID: {guid}");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve commodity interface for ItemId: {_itemID}");
                    writelog($"Could not retrieve commodity interface for ItemId: {_itemID}");
                }

                i++;
            }

        }
        #endregion

        #region Pen

        public async Task<string> PairingPen()
        {
            _itemID = new ItemId(PenItemID);
            if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_penInterfaceType, commodity, "Pair");

                if (value == null)
                {
                    Debug.WriteLine("Pen Pair value is null.");
                    writelog("Pen Pair value is null.");
                    return "";
                }
                else if (value is string stringValue)
                {
                    Debug.WriteLine($"Pen Pair value: {stringValue}");
                    writelog($"Pen Pair value: {stringValue}");
                    return stringValue;
                }
                else
                {
                    Debug.WriteLine("Pen Pair value is not a string.");
                    writelog("Pen Pair value is not a string.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                return "";
            }
        }
        public async Task UnPairPen(string Guid)
        {
            if (!await GetItemIDAsync("Pen", Guid))
            { return; }

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "UnPair", true);
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[UnPairPen]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[UnPairPen]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task<JArray> GetPenDeviceItemsEx()
        {
            _itemID = new ItemId(PenItemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "DeviceItemsEx");

                    if (value == null)
                    {
                        Debug.WriteLine("Pen DeviceItemsEx value is null.");
                        writelog("Pen DeviceItemsEx value is null.");
                        return new JArray();
                    }
                    else if (value is JArray jArrayValue)
                    {
                        Debug.WriteLine("Pen DeviceItemsEx value retrieved successfully.");
                        writelog("Pen DeviceItemsEx value retrieved successfully.");
                        return jArrayValue;
                    }
                    else
                    {
                        Debug.WriteLine("Pen DeviceItemsEx value is not a JArray.");
                        writelog("Pen DeviceItemsEx value is not a JArray.");
                        return new JArray();
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return new JArray();
                }
            }
            else
            {
                Debug.WriteLine($"[GetDeviceItemsEx]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetDeviceItemsEx]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return new JArray();
            }

        }
        public async Task<string> GetEraserDoublePressValues()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "EraserDoublePressValues");

                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine($"EraserDoublePressValues: {str}");
                        writelog($"EraserDoublePressValues: {str}");
                        return str;
                    }
                    else
                    {
                        Debug.WriteLine("EraserDoublePressValues is not a byte array or is null.");
                        writelog("EraserDoublePressValues is not a byte array or is null.");
                        return string.Empty;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetEraserDoublePressValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetEraserDoublePressValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetEraserSinglePressValues()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "EraserSinglePressValues");

                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine($"EraserSinglePressValues: {str}");
                        writelog($"EraserSinglePressValues: {str}");
                        return str;
                    }
                    else
                    {
                        Debug.WriteLine("EraserSinglePressValues is not a byte array or is null.");
                        writelog("EraserSinglePressValues is not a byte array or is null.");
                        return string.Empty;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetEraserSinglePressValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetEraserSinglePressValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetEraserLongPressValues()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "EraserLongPressValues");
                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine(str);
                        return str;
                    }
                    return string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetEraserLongPressValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetEraserLongPressValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetSideSwitchSinglePressValues()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "SideSwitchSinglePressValues");
                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine(str);
                        return str;
                    }
                    return string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetSideSwitchSinglePressValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetSideSwitchSinglePressValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetMenuSinglePressValues()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "MenuSinglePressValues");
                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine(str);
                        return str;
                    }
                    return string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetMenuSinglePressValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetMenuSinglePressValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetLaunchableAppValues()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "LaunchableAppValues");
                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine(str);
                        return str;
                    }
                    return string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetLaunchableAppValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetLaunchableAppValues]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetEraserDoublePressSetting()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "EraserDoublePressSetting");
                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine(str);
                        return str;
                    }
                    return string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetEraserDoublePressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetEraserDoublePressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetEraserSinglePressSetting()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "EraserSinglePressSetting");
                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine(str);
                        return str;
                    }
                    return string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetEraserSinglePressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetEraserSinglePressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetEraserLongPressSetting()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "EraserLongPressSetting");
                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine(str);
                        return str;
                    }
                    return string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetEraserLongPressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetEraserLongPressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetSideTopSwitchSinglePressSetting()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "SideTopSwitchSinglePressSetting");
                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine(str);
                        return str;
                    }
                    return string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetEraserDGetSideTopSwitchSinglePressSettingoublePressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetSideTopSwitchSinglePressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetSideBottomSwitchSinglePressSetting()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "SideBottomSwitchSinglePressSetting");
                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine(str);
                        return str;
                    }
                    return string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetSideBottomSwitchSinglePressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetSideBottomSwitchSinglePressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }

        }
        public async Task<string> GetMenuSinglePressSetting()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "MenuSinglePressSetting");
                    if (value is byte[] byteArray)
                    {
                        var str = Encoding.UTF8.GetString(byteArray);
                        Debug.WriteLine(str);
                        return str;
                    }
                    return string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return "";
                }
            }
            else
            {
                Debug.WriteLine($"[GetMenuSinglePressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetMenuSinglePressSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return "";
            }
        }
        public async Task<bool> GetMenuCenterRightClickSetting()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "MenuCenterRightClickSetting");

                    if (value is bool boolValue)
                    {
                        Debug.WriteLine($"MenuCenterRightClickSetting: {boolValue}");
                        writelog($"MenuCenterRightClickSetting: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("MenuCenterRightClickSetting is not a boolean or is null.");
                        writelog("MenuCenterRightClickSetting is not a boolean or is null.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[GetMenuCenterRightClickSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetMenuCenterRightClickSetting]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return false;
            }

        }
        public async Task<bool> GetIsSideTopButtonHoverClick()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "IsSideTopButtonHoverClick");

                    if (value is bool boolValue)
                    {
                        Debug.WriteLine($"IsSideTopButtonHoverClick: {boolValue}");
                        writelog($"IsSideTopButtonHoverClick: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("IsSideTopButtonHoverClick is not a boolean or is null.");
                        writelog("IsSideTopButtonHoverClick is not a boolean or is null.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsSideTopButtonHoverClick]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetIsSideTopButtonHoverClick]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return false;
            }
        }
        public async Task<bool> GetIsSideBottomButtonHoverClick()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "IsSideBottomButtonHoverClick");

                    if (value is bool boolValue)
                    {
                        Debug.WriteLine($"IsSideBottomButtonHoverClick: {boolValue}");
                        writelog($"IsSideBottomButtonHoverClick: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("IsSideBottomButtonHoverClick is not a boolean or is null.");
                        writelog("IsSideBottomButtonHoverClick is not a boolean or is null.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsSideBottomButtonHoverClick]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetIsSideBottomButtonHoverClick]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return false;
            }

        }
        public async Task<bool> StartKeyCapturePen()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "StartKeyCapture");

                    if (value is bool boolValue)
                    {
                        Debug.WriteLine($"StartKeyCapture: {boolValue}");
                        writelog($"StartKeyCapture: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("StartKeyCapture is not a boolean or is null.");
                        writelog("StartKeyCapture is not a boolean or is null.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[StartKeyCapturePen]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[StartKeyCapturePen]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return false;
            }

        }
        public async Task<bool> FinishKeyCapturePen()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "FinishKeyCapture");

                    if (value is bool boolValue)
                    {
                        Debug.WriteLine($"FinishKeyCapture: {boolValue}");
                        writelog($"FinishKeyCapture: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        Debug.WriteLine("FinishKeyCapture is not a boolean or is null.");
                        writelog("FinishKeyCapture is not a boolean or is null.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[FinishKeyCapturePen]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[FinishKeyCapturePen]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return false;
            }

        }
        public async Task<string> KeyCaptureData()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "KeyCaptureData");

                    if (value == null)
                    {
                        Debug.WriteLine("KeyCaptureData is null.");
                        writelog("KeyCaptureData is null.");
                        return "";
                    }
                    else if (value is string stringValue)
                    {
                        Debug.WriteLine($"KeyCaptureData: {stringValue}");
                        writelog($"KeyCaptureData: {stringValue}");
                        return stringValue;
                    }
                    else
                    {
                        Debug.WriteLine("KeyCaptureData is not a string.");
                        writelog("KeyCaptureData is not a string.");
                        return "";
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return string.Empty;
                }
            }
            else
            {
                Debug.WriteLine($"[KeyCaptureData]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[KeyCaptureData]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return string.Empty;
            }

        }
        public async Task<string> GetIsdDriverVersion()
        {
            _itemID = new ItemId(PenItemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "IsdDriverVersion");

                    if (value == null)
                    {
                        Debug.WriteLine("IsdDriverVersion is null.");
                        writelog("IsdDriverVersion is null.");
                        return "";
                    }
                    else if (value is string stringValue)
                    {
                        Debug.WriteLine($"IsdDriverVersion: {stringValue}");
                        writelog($"IsdDriverVersion: {stringValue}");
                        return stringValue;
                    }
                    else
                    {
                        Debug.WriteLine("IsdDriverVersion is not a string.");
                        writelog("IsdDriverVersion is not a string.");
                        return "";
                    }
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return string.Empty;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsdDriverVersion]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetIsdDriverVersion]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return string.Empty;
            }

        }

        public async Task SetEraserDoublePressSetting(string itemID, byte[] newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "EraserDoublePressSetting", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetEraserDoublePressSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetEraserDoublePressSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetEraserLongPressSetting(string itemID, byte[] newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "EraserLongPressSetting", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetEraserLongPressSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetEraserLongPressSettingCould not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetEraserSinglePressSetting(string itemID, byte[] newValue)
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "EraserSinglePressSetting", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetEraserSinglePressSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetEraserSinglePressSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetIsSideBottomButtonHoverClick(string itemID, bool newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "IsSideBottomButtonHoverClick", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetIsSideBottomButtonHoverClick]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetIsSideBottomButtonHoverClick]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetIsSideTopButtonHoverClick(string itemID, bool newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "IsSideTopButtonHoverClick", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetIsSideTopButtonHoverClick]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetIsSideTopButtonHoverClick]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetMenuSinglePressSetting(string itemID, byte[] newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "MenuSinglePressSetting", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetMenuSinglePressSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetMenuSinglePressSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetMenuCenterRightClickSetting(string itemID, bool newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "MenuCenterRightClickSetting", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetMenuCenterRightClickSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetMenuCenterRightClickSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetSideBottomSwitchSinglePressSetting(string itemID, byte[] newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "SideBottomSwitchSinglePressSetting", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetSideBottomSwitchSinglePressSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetSideBottomSwitchSinglePressSetting]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetSideTopSwitchSinglePressSetting(string itemID, byte[] newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "SideTopSwitchSinglePressSetting", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetSideTopSwitchSinglePressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetSideTopSwitchSinglePressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetTiltSensitivity(string itemID, int newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "TiltSensitivity", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetTiltSensitivity]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetTiltSensitivity]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetTipSensitivity(string itemID, int newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "TipSensitivity", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not set the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetTipSensitivity]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetTipSensitivity]Could not set the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }
        public async Task<bool> RestoreToDefaultPen()
        {
            if (!IsDTPReady)
                return false;

            byte[] newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":73,\"actionName\":\"\"}}");
            await SetEraserSinglePressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":90,\"actionName\":\"\"}}");
            await SetEraserDoublePressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":75,\"actionName\":\"\"}}");
            await SetEraserLongPressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":27,\"actionName\":\"\"}}");
            await SetSideTopSwitchSinglePressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":26,\"actionName\":\"\"}}");
            await SetSideBottomSwitchSinglePressSetting(PenItemID0, newValue);
            await SetTipSensitivity(PenItemID0, 3);
            await SetTiltSensitivity(PenItemID0, 0);
            await SetIsSideTopButtonHoverClick(PenItemID0, false);
            await SetIsSideBottomButtonHoverClick(PenItemID0, false);

            await RestoreRadialMenuToDefault();

            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\Actions\pen.json");
            if (File.Exists(filePath))
            {
                try
                {
                    if (DDPMFileSecurity.ValidateFilePath(filePath, out string info))
                        File.Delete(filePath);
                    else
                        writelog($"[DTPProxyPlugin][RestoreToDefaultPen][Path] Delete setting file failed: {info}");
                }
                catch (Exception ex)
                {
                    writelog($"[DTPProxyPlugin][RestoreToDefaultPen] Delete setting file failed: {ex}");
                }
            }
            var message = $"Pen|RestoreToDefault||";
            SendDTPEventToUI(message);
            return true;
        }
        public async Task<bool> RestoreRadialMenuToDefault()
        {
            if (!IsDTPReady)
                return false;

            byte[] newValue = Encoding.UTF8.GetBytes($"{{\"menuIndex\":0,\"actionId\":82,\"actionName\":\"\"}}");
            await SetMenuSinglePressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"menuIndex\":1,\"actionId\":79,\"actionName\":\"\"}}");
            await SetMenuSinglePressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"menuIndex\":2,\"actionId\":86,\"actionName\":\"\"}}");
            await SetMenuSinglePressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"menuIndex\":3,\"actionId\":80,\"actionName\":\"\"}}");
            await SetMenuSinglePressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"menuIndex\":4,\"actionId\":83,\"actionName\":\"\"}}");
            await SetMenuSinglePressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"menuIndex\":5,\"actionId\":85,\"actionName\":\"\"}}");
            await SetMenuSinglePressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"menuIndex\":6,\"actionId\":81,\"actionName\":\"\"}}");
            await SetMenuSinglePressSetting(PenItemID0, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"menuIndex\":7,\"actionId\":84,\"actionName\":\"\"}}");
            await SetMenuSinglePressSetting(PenItemID0, newValue);
            await SetMenuCenterRightClickSetting(PenItemID0, true);
            return true;
        }

        #endregion

        #region Headset set

        // 1125 Add log
        public async Task<bool> SetMicNoiseCancellationAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "MicNoiseCancellation", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetMicNoiseCancellationAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetMicNoiseCancellationAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetSidetoneAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "Sidetone", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetSidetoneAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetSidetoneAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBusyLightAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "BusyLight", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetBusyLightAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetBusyLightAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetVoiceGuidanceAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "VoiceGuidance", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetVoiceGuidanceAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetVoiceGuidanceAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetSelectedPresetAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "SelectedPreset", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetSelectedPresetAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetSelectedPresetAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetSidetoneLevelAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "SidetoneLevel", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetSidetoneLevelAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetSidetoneLevelAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBandsGainAsync(string Guid, byte[] newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "BandsGain", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetBandsGainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetBandsGainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBand1GainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "Band1Gain", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetBand1GainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetBand1GainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBand2GainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "Band2Gain", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetBand2GainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetBand2GainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBand3GainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "Band3Gain", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetBand3GainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetBand3GainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBand4GainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "Band4Gain", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetBand4GainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetBand4GainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBand5GainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "Band5Gain", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetBand5GainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetBand5GainAsync failed: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> SetAncModeAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "AncMode", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetAncModeAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetAncModeAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAncGainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "AncGain", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetAncGainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetAncGainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetWearDetectionAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "WearDetection", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetWearDetectionAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetWearDetectionAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetIsWearDetectionMuteMicEnabledAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionMuteMicEnabled", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetIsWearDetectionMuteMicEnabledAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetIsWearDetectionMuteMicEnabledAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetIsWearDetectionPauseMusicEnabledAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionPauseMusicEnabled", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetIsWearDetectionPauseMusicEnabledAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetWearDetectionAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetWearDetectionQuickPauseAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "WearDetectionQuickPause", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetWearDetectionQuickPauseAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetAncGainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetWearDetectionSensitivityAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "WearDetectionSensitivity", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetWearDetectionSensitivityAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetAncGainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetMicNCIncomingAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "MicNCIncoming", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetMicNCIncomingAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetMicNCIncomingAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetUnPairAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "UnPair", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetUnPairAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetUnPairAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetFactoryResetAsyncValueForHeadset(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "FactoryReset", newValue);
                    SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_SetFactoryResetAsyncValueForHeadset",
                        Guid, $"Headset_SetFactoryResetAsyncValueForHeadset:{newValue.ToString()}"));
                    writelog("[DTPProxyPlugin] [Headset] SetFactoryResetAsyncValueForHeadset Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetFactoryResetAsyncValueForHeadset failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetFactoryResetAsyncValueForHeadsetForCLI(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "FactoryReset", newValue);
                    SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_SetFactoryResetAsyncValueForHeadsetForCLI",
                        Guid, $"Headset_SetFactoryResetAsyncValueForHeadsetForCLI:{newValue.ToString()}"));
                    writelog("[DTPProxyPlugin] [Headset] Headset_SetFactoryResetAsyncValueForHeadsetForCLI Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetFactoryResetAsyncValueForHeadset failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBoomMicAsync(string Guid, bool newValue)
        {
            string guidString = Guid;
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "BoomMic", newValue);
                    writelog("[DTPProxyPlugin] [Headset] SetBoomMicAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [Headset] Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] SetBoomMicAsync failed: {ex.Message}");
                return false;
            }
        }

        #endregion Headset set

        #region Headset Get

        public async Task<JArray> GetHeadsetDeviceItemsExAsync()
        {
            try
            {
                _itemID = new ItemId(HeadsetItemID);

                if (_headsetMethodInfo != null)
                {
                    var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                    if (commodity is ICommodity)
                    {
                        var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceItemsEx");
                        writelog($"[DTPProxyPlugin] [Headset] GetDeviceItemsExAsync succeeded");
                        return value == null ? new JArray() : (JArray)value;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetDeviceItemsExAsync failed: Could not retrieve commodity interface");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetDeviceItemsExAsync failed - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<DeviceInterfaceType> GetHeadsetInterfaceTypeAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return default;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "InterfaceType");

                    if (value is DeviceInterfaceType deviceInterfaceType)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetInterfaceTypeAsync succeeded for {guid} with value: {deviceInterfaceType}");
                        return deviceInterfaceType;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetInterfaceTypeAsync failed for {guid}. Value is not of type DeviceInterfaceType.");
                        return default(DeviceInterfaceType); // or handle the error as needed
                    }

                }

                writelog($"[DTPProxyPlugin] [Headset] GetInterfaceTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return default;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetInterfaceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return default;
            }
        }

        public async Task<string> GetHeadsetDeviceNameAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceName");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetDeviceNameAsync failed for {guid}. DeviceName is null.");
                        return "";
                    }
                    else if (value is string stringValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetDeviceNameAsync succeeded for {guid} with value: {stringValue}");
                        return stringValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetDeviceNameAsync failed for {guid}. DeviceName is not a string.");
                        return "";
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetDeviceNameAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetDeviceNameAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetDeviceIdAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceId");
                    writelog($"[DTPProxyPlugin] [Headset] GetDeviceIdAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetDeviceIdAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetDeviceIdAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetPluginIdAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PluginId");
                    writelog($"[DTPProxyPlugin] [Headset] GetPluginIdAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetPluginIdAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetPluginIdAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetHeadsetODMIdAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ODMId");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetODMIdAsync failed for {guid}. ODMId is null.");
                        return -1;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetODMIdAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetODMIdAsync failed for {guid}. ODMId is not an integer.");
                        return -1;
                    }

                }

                writelog($"[DTPProxyPlugin] [Headset] GetODMIdAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetODMIdAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetHeadsetModelNumberAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ModelNumber");
                    writelog($"[DTPProxyPlugin] [Headset] GetModelNumberAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetModelNumberAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetModelNumberAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetHeadsetInstanceNumberAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "InstanceNumber");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetInstanceNumberAsync failed for {guid}. InstanceNumber is null.");
                        return -1;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetInstanceNumberAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetInstanceNumberAsync failed for {guid}. InstanceNumber is not an integer.");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetInstanceNumberAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetInstanceNumberAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetHeadsetInstanceIdAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "InstanceId");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetInstanceIdAsync failed for {guid}. InstanceId is null.");
                        return -1;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetInstanceIdAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetInstanceIdAsync failed for {guid}. InstanceId is not an integer.");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetInstanceIdAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetInstanceIdAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetHeadsetFirmwareVersionAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "FirmwareVersion");
                    writelog($"[DTPProxyPlugin] [Headset] GetFirmwareVersionAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetFirmwareVersionAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetFirmwareVersionAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetDeviceTypeAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceType");
                    writelog($"[DTPProxyPlugin] [Headset] GetDeviceTypeAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetDeviceTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetDeviceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetParentDeviceTypeAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ParentDeviceType");
                    writelog($"[DTPProxyPlugin] [Headset] GetParentDeviceTypeAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetParentDeviceTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetParentDeviceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GetHeadsetIsBatteryLevelSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsBatteryLevelSupported");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsBatteryLevelSupportedAsync failed for {guid}. IsBatteryLevelSupported is null.");
                        return false;
                    }
                    else if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsBatteryLevelSupportedAsync succeeded for {guid} with value: {boolValue}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsBatteryLevelSupportedAsync failed for {guid}. IsBatteryLevelSupported is not a boolean.");
                        return false;
                    }

                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsBatteryLevelSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsBatteryLevelSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetHeadsetBatteryLevelAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "BatteryLevel");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBatteryLevelAsync failed for {guid}. BatteryLevel is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBatteryLevelAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBatteryLevelAsync failed for {guid}. BatteryLevel is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [Headset] GetBatteryLevelAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetBatteryLevelAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<string> GetHeadsetDeviceBatteryStatusAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceBatteryStatus");
                    writelog($"[DTPProxyPlugin] [Headset] GetDeviceBatteryStatusAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetDeviceBatteryStatusAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetDeviceBatteryStatusAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetPairingStatusAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairingStatus");
                    writelog($"[DTPProxyPlugin] [Headset] GetPairingStatusAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetPairingStatusAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetPairingStatusAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetPairedHostName1Async(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairedHostName1");
                    writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName1Async succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName1Async failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName1Async failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetPairedHostName2Async(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairedHostName2");
                    writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName2Async succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName2Async failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName2Async failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetPairedHostName3Async(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairedHostName3");
                    writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName3Async succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName3Async failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName3Async failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetHeadsetMaxPairingSlotsAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "MaxPairingSlots");
                    writelog($"[DTPProxyPlugin] [Headset] GetMaxPairingSlotsAsync succeeded for {guid}");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] MaxPairingSlots failed for {guid}.  is null.");
                        return -1;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] MaxPairingSlots succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] MaxPairingSlots failed for {guid}. is not an integer.");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetMaxPairingSlotsAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetMaxPairingSlotsAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetHeadsetPairedDeviceCountAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairedDeviceCount");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetPairedDeviceCountAsync: PairedDeviceCount is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetPairedDeviceCountAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetPairedDeviceCountAsync: PairedDeviceCount is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetPairedDeviceCountAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetPairedDeviceCountAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }
        #region Headset Get (continued)

        public async Task<int> GetHeadsetTotalNumberOfPairedHostNameAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "TotalNumberOfPairedHostName");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetTotalNumberOfPairedHostNameAsync: TotalNumberOfPairedHostName is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetTotalNumberOfPairedHostNameAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetTotalNumberOfPairedHostNameAsync: TotalNumberOfPairedHostName is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetTotalNumberOfPairedHostNameAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetTotalNumberOfPairedHostNameAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetHeadsetSerialNumberAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "SerialNumber");
                    writelog($"[DTPProxyPlugin] [Headset] GetSerialNumberAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetSerialNumberAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetSerialNumberAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GetIsReadyAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsReady");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsReadyAsync: IsReady is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsReadyAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsReadyAsync: IsReady is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsReadyAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsReadyAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsDirtyAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsDirty");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsDirtyAsync: IsDirty is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsDirtyAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsDirtyAsync: IsDirty is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsDirtyAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsDirtyAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsMicNoiseCancellationSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsMicNoiseCancellationSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsMicNoiseCancellationSupportedAsync: IsMicNoiseCancellationSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsMicNoiseCancellationSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsMicNoiseCancellationSupportedAsync: IsMicNoiseCancellationSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsMicNoiseCancellationSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsMicNoiseCancellationSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsSidetoneSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsSidetoneSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsSidetoneSupportedAsync: IsSidetoneSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsSidetoneSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsSidetoneSupportedAsync: IsSidetoneSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsSidetoneSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsSidetoneSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsBusyLightSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsBusyLightSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsBusyLightSupportedAsync: IsBusyLightSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsBusyLightSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsBusyLightSupportedAsync: IsBusyLightSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsBusyLightSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsBusyLightSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsVoiceGuidanceSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsVoiceGuidanceSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsVoiceGuidanceSupportedAsync: IsVoiceGuidanceSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsVoiceGuidanceSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsVoiceGuidanceSupportedAsync: IsVoiceGuidanceSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsVoiceGuidanceSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsVoiceGuidanceSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsPresetsSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsPresetsSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsPresetsSupportedAsync: IsPresetsSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsPresetsSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsPresetsSupportedAsync: IsPresetsSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsPresetsSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsPresetsSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsEqualizerSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsEqualizerSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsEqualizerSupportedAsync: IsEqualizerSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsEqualizerSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsEqualizerSupportedAsync: IsEqualizerSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsEqualizerSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsEqualizerSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<HeadsetConnectionType> GetConnectionTypeAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return HeadsetConnectionType.HeadsetConnectionTypeUnknown;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ConnectionType");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetConnectionTypeAsync: ConnectionType is null for {guid}");
                        return default(HeadsetConnectionType);
                    }

                    if (value is HeadsetConnectionType connectionType)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetConnectionTypeAsync succeeded for {guid}");
                        return connectionType;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetConnectionTypeAsync: ConnectionType is not of type HeadsetConnectionType for {guid}");
                        return default(HeadsetConnectionType);
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetConnectionTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return HeadsetConnectionType.HeadsetConnectionTypeUnknown;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetConnectionTypeAsync failed for {guid} - Exception: {ex.Message}");
                return HeadsetConnectionType.HeadsetConnectionTypeUnknown;
            }
        }

        public async Task<bool> GetIsANCSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsANCSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsANCSupportedAsync: IsANCSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsANCSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsANCSupportedAsync: IsANCSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsANCSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsANCSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSupportedAsync: IsWearDetectionSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSupportedAsync: IsWearDetectionSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionSensitivitySupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionSensitivitySupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSensitivitySupportedAsync: IsWearDetectionSensitivitySupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSensitivitySupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSensitivitySupportedAsync: IsWearDetectionSensitivitySupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSensitivitySupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSensitivitySupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionPauseMusicSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionPauseMusicSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicSupportedAsync: IsWearDetectionPauseMusicSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicSupportedAsync: IsWearDetectionPauseMusicSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionMuteMicSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionMuteMicSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicSupportedAsync: IsWearDetectionMuteMicSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicSupportedAsync: IsWearDetectionMuteMicSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionQuickPauseSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionQuickPauseSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionQuickPauseSupportedAsync: IsWearDetectionQuickPauseSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionQuickPauseSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionQuickPauseSupportedAsync: IsWearDetectionQuickPauseSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionQuickPauseSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionQuickPauseSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetMicNoiseCancellationAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "MicNoiseCancellation");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetMicNoiseCancellationAsync: MicNoiseCancellation is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetMicNoiseCancellationAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetMicNoiseCancellationAsync: MicNoiseCancellation is not a boolean for {guid}");
                        return false;
                    }

                }

                writelog($"[DTPProxyPlugin] [Headset] GetMicNoiseCancellationAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetMicNoiseCancellationAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetMicNCIncomingAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "MicNCIncoming");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetMicNCIncomingAsync: MicNCIncoming is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetMicNCIncomingAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetMicNCIncomingAsync: MicNCIncoming is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetMicNCIncomingAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetMicNCIncomingAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetSidetoneAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Sidetone");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetSidetoneAsync: Sidetone is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetSidetoneAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetSidetoneAsync: Sidetone is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetSidetoneAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetSidetoneAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetBusyLightAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "BusyLight");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBusyLightAsync: BusyLight is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBusyLightAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBusyLightAsync: BusyLight is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetBusyLightAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetBusyLightAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetVoiceGuidanceAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "VoiceGuidance");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetVoiceGuidanceAsync: VoiceGuidance is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetVoiceGuidanceAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetVoiceGuidanceAsync: VoiceGuidance is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetVoiceGuidanceAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetVoiceGuidanceAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetSelectedPresetAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "SelectedPreset");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetSelectedPresetAsync: SelectedPreset is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetSelectedPresetAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetSelectedPresetAsync: SelectedPreset is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetSelectedPresetAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetSelectedPresetAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetSidetoneLevelAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "SidetoneLevel");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetSidetoneLevelAsync: SidetoneLevel is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetSidetoneLevelAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetSidetoneLevelAsync: SidetoneLevel is not an integer for {guid}");
                        return -1;
                    }

                }

                writelog($"[DTPProxyPlugin] [Headset] GetSidetoneLevelAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetSidetoneLevelAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetMuteStatusAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "MuteStatus");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetMuteStatusAsync: MuteStatus is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetMuteStatusAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetMuteStatusAsync: MuteStatus is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetMuteStatusAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetMuteStatusAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]> GetBandsGainAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "BandsGain");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBandsGainAsync: BandsGain is null for {guid}");
                        return null;
                    }

                    if (value is byte[] byteArray)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBandsGainAsync succeeded for {guid}");
                        return byteArray;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBandsGainAsync: BandsGain is not a byte array for {guid}");
                        return null;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetBandsGainAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetBandsGainAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetBand1GainAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Band1Gain");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand1GainAsync: Band1Gain is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand1GainAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand1GainAsync: Band1Gain is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetBand1GainAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetBand1GainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetBand2GainAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Band2Gain");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand2GainAsync: Band2Gain is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand2GainAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand2GainAsync: Band2Gain is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetBand2GainAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetBand2GainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetBand3GainAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Band3Gain");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand3GainAsync: Band3Gain is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand3GainAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand3GainAsync: Band3Gain is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetBand3GainAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetBand3GainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetBand4GainAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Band4Gain");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand4GainAsync: Band4Gain is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand4GainAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand4GainAsync: Band4Gain is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetBand4GainAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetBand4GainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetBand5GainAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Band5Gain");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand5GainAsync: Band5Gain is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand5GainAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBand5GainAsync: Band5Gain is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetBand5GainAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetBand5GainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAncModeAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "AncMode");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetAncModeAsync: AncMode is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetAncModeAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetAncModeAsync: AncMode is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetAncModeAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetAncModeAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAncGainAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "AncGain");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetAncGainAsync: AncGain is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetAncGainAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetAncGainAsync: AncGain is not an integer for {guid}");
                        return -1;
                    }

                }

                writelog($"[DTPProxyPlugin] [Headset] GetAncGainAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetAncGainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetWearDetectionAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "WearDetection");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionAsync: WearDetection is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionAsync: WearDetection is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionPauseMusicEnabledAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionPauseMusicEnabled");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicEnabledAsync: IsWearDetectionPauseMusicEnabled is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicEnabledAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicEnabledAsync: IsWearDetectionPauseMusicEnabled is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicEnabledAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicEnabledAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionMuteMicEnabledAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionMuteMicEnabled");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicEnabledAsync: IsWearDetectionMuteMicEnabled is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicEnabledAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicEnabledAsync: IsWearDetectionMuteMicEnabled is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicEnabledAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicEnabledAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetWearDetectionSensitivityAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "WearDetectionSensitivity");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionSensitivityAsync: WearDetectionSensitivity is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionSensitivityAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionSensitivityAsync: WearDetectionSensitivity is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionSensitivityAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionSensitivityAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetWearDetectionQuickPauseAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "WearDetectionQuickPause");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionQuickPauseAsync: WearDetectionQuickPause is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionQuickPauseAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionQuickPauseAsync: WearDetectionQuickPause is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionQuickPauseAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionQuickPauseAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetIsMicNCIncomingSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsMicNCIncomingSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsMicNCIncomingSupportedAsync: IsMicNCIncomingSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsMicNCIncomingSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsMicNCIncomingSupportedAsync: IsMicNCIncomingSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsMicNCIncomingSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsMicNCIncomingSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsBoomMicSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsBoomMicSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsBoomMicSupportedAsync: IsBoomMicSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsBoomMicSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetIsBoomMicSupportedAsync: IsBoomMicSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetIsBoomMicSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetIsBoomMicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetBoomMicAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "BoomMic");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBoomMicAsync: BoomMic is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBoomMicAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [Headset] GetBoomMicAsync: BoomMic is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [Headset] GetBoomMicAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetBoomMicAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        #endregion Headset Get

        #endregion

        #region Headset Event

        #region Headset Already connected do this
        /// <summary>
        /// Already connected do this
        /// </summary>
        /// <returns></returns>
        private async Task<bool> RegisterEventsForAllHeadsetAsync()
        {
            bool result = false;
            var headsets = await GetHeadsetDevsCountAsync();
            if (headsets > 0)
            {
                writelog($"Headset instance count: {headsets} to register");

                for (int i = 0; i < headsets; i++)
                {
                    result = await RegisterEventsForHeadsetAsync(i);

                    if (!result)
                    {
                        writelog($"[Headset] Register Events For Headset{i} fail, try un-register and register again");

                        result = await UnregisterEventsForHeadsetAsync(i);
                        result = await RegisterEventsForHeadsetAsync(i);
                        writelog($"[Headset] Retry register result is {result}");
                    }
                }
            }
            else
            {
                writelog($"[Headset] No any headset instance to register.");
                return false;
                //try to force release current --> will catch exception  1123
                //await UnregisterEventsForWebcamAsync(0);
            }
            return false;
        }

        /// <summary>
        /// Already connected do this
        /// </summary>
        /// <returns></returns>
        private async Task UnregisterEventsForAllHeadsetAsync()
        {
            bool result = false;
            var headsets = await GetHeadsetDevsCountAsync();
            if (headsets > 0)
            {
                writelog($"[Headset] instance count: {headsets} to unregister.");

                for (int i = headsets - 1; i >= 0; i--)
                {
                    result = await UnregisterEventsForHeadsetAsync(i);
                }
            }
            else
                writelog($"[Headset] No any headset instance to unregister.");
        }

        private async Task<bool> RegisterEventsForHeadsetAsync(int index)
        {
            if (null == _commSdk || null == _comdity || index < 0)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<IHeadsetCommodity>(new ItemId($"DellPeripheral.Headset.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity _Headsetcom)
                {
                    _Headsetcom.FirmwareVersionChanged += Headset_FirmwareVersionChanged;
                    _Headsetcom.BatteryLevelChanged += Headset_BatteryLevelChanged;
                    _Headsetcom.BatteryStatusChanged += Headset_BatteryStatusChanged;
                    _Headsetcom.PairedHostNameChanged += Headset_PairedHostNameChanged;
                    _Headsetcom.InstanceNumberChanged += Headset_InstanceNumberChanged;
                    _Headsetcom.IsReadyChanged += Headset_IsReadyChanged;
                    _Headsetcom.IsDirtyChanged += Headset_IsDirtyChanged;
                    _Headsetcom.MicNoiseCancellationChanged += Headset_MicNoiseCancellationChanged;
                    _Headsetcom.MicNCIncomingChanged += Headset_MicNCIncomingChanged;
                    _Headsetcom.SidetoneChanged += Headset_SidetoneChanged;
                    _Headsetcom.BusyLightChanged += Headset_BusyLightChanged;
                    _Headsetcom.VoiceGuidanceChanged += Headset_VoiceGuidanceChanged;
                    _Headsetcom.SelectedPresetChanged += Headset_SelectedPresetChanged;
                    _Headsetcom.SidetoneLevelChanged += Headset_SidetoneLevelChanged;
                    _Headsetcom.MuteStatusChanged += Headset_MuteStatusChanged;
                    _Headsetcom.BandsGainChanged += Headset_BandsGainChanged;
                    _Headsetcom.AncModeChanged += Headset_AncModeChanged;
                    _Headsetcom.AncGainChanged += Headset_AncGainChanged;
                    _Headsetcom.BoomMicChanged += Headset_BoomMicChanged;
                    _Headsetcom.IsBoomMicSupportedChanged += Headset_BoomMicSupportedChanged;
                    _Headsetcom.SerialNumberChanged += Headset_SerialNumberChanged;
                    _Headsetcom.SerialNumberChanged += Headset_SerialNumberChangedForCMA;
                    _Headsetcom.WearDetectionChanged += Headset_WearDetectionChanged;
                    _Headsetcom.IsWearDetectionPauseMusicEnabledChanged += Headset_IsWearDetectionPauseMusicEnabledChanged;
                    _Headsetcom.IsWearDetectionMuteMicEnabledChanged += Headset_IsWearDetectionMuteMicEnabledChanged;
                    _Headsetcom.WearDetectionSensitivityChanged += Headset_WearDetectionSensitivityChanged;
                    _Headsetcom.WearDetectionQuickPauseChanged += Headset_WearDetectionQuickPauseChanged;

                    writelog($"Headset{index} Commodity events registered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"[Headset] {index} RegisterEventsForHeadset Exception {e.Message}");

                return false;
            }

            return false;
        }

        private async Task<bool> UnregisterEventsForHeadsetAsync(int index)
        {
            if (null == _commSdk || null == _comdity || index < 0)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<IHeadsetCommodity>(new ItemId($"DellPeripheral.Headset.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity _Headsetcom)
                {
                    _Headsetcom.FirmwareVersionChanged -= Headset_FirmwareVersionChanged;
                    _Headsetcom.BatteryLevelChanged -= Headset_BatteryLevelChanged;
                    _Headsetcom.BatteryStatusChanged -= Headset_BatteryStatusChanged;
                    _Headsetcom.PairedHostNameChanged -= Headset_PairedHostNameChanged;
                    _Headsetcom.InstanceNumberChanged -= Headset_InstanceNumberChanged;
                    _Headsetcom.IsReadyChanged -= Headset_IsReadyChanged;
                    _Headsetcom.IsDirtyChanged -= Headset_IsDirtyChanged;
                    _Headsetcom.MicNoiseCancellationChanged -= Headset_MicNoiseCancellationChanged;
                    _Headsetcom.MicNCIncomingChanged -= Headset_MicNCIncomingChanged;
                    _Headsetcom.SidetoneChanged -= Headset_SidetoneChanged;
                    _Headsetcom.BusyLightChanged -= Headset_BusyLightChanged;
                    _Headsetcom.VoiceGuidanceChanged -= Headset_VoiceGuidanceChanged;
                    _Headsetcom.SelectedPresetChanged -= Headset_SelectedPresetChanged;
                    _Headsetcom.SidetoneLevelChanged -= Headset_SidetoneLevelChanged;
                    _Headsetcom.MuteStatusChanged -= Headset_MuteStatusChanged;
                    _Headsetcom.BandsGainChanged -= Headset_BandsGainChanged;
                    _Headsetcom.AncModeChanged -= Headset_AncModeChanged;
                    _Headsetcom.AncGainChanged -= Headset_AncGainChanged;
                    _Headsetcom.BoomMicChanged -= Headset_BoomMicChanged;
                    _Headsetcom.IsBoomMicSupportedChanged -= Headset_BoomMicSupportedChanged;
                    _Headsetcom.SerialNumberChanged -= Headset_SerialNumberChanged;
                    _Headsetcom.SerialNumberChanged -= Headset_SerialNumberChangedForCMA;
                    _Headsetcom.WearDetectionChanged -= Headset_WearDetectionChanged;
                    _Headsetcom.IsWearDetectionPauseMusicEnabledChanged -= Headset_IsWearDetectionPauseMusicEnabledChanged;
                    _Headsetcom.IsWearDetectionMuteMicEnabledChanged -= Headset_IsWearDetectionMuteMicEnabledChanged;
                    _Headsetcom.WearDetectionSensitivityChanged -= Headset_WearDetectionSensitivityChanged;
                    _Headsetcom.WearDetectionQuickPauseChanged -= Headset_WearDetectionQuickPauseChanged;

                    writelog($"[Headset] Headset{index} Commodity events unregistered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"[Headset] Headset{index} UnregisterEventsForHeadset Exception {e.Message}");

                return false;
            }
            return false;
        }

        #endregion  Headset Already connected do this

        private bool UnregisterEventsForHeadset(HeadsetEventHandleObject obj)
        {
            writelog($"[Headset] UnregisterEventsForHeadset in ... ");
            if (obj.headsetCommodity is Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity _Headsetcom)
            {
                _Headsetcom.FirmwareVersionChanged -= Headset_FirmwareVersionChanged;
                _Headsetcom.BatteryLevelChanged -= Headset_BatteryLevelChanged;
                _Headsetcom.BatteryStatusChanged -= Headset_BatteryStatusChanged;
                _Headsetcom.PairedHostNameChanged -= Headset_PairedHostNameChanged;
                _Headsetcom.InstanceNumberChanged -= Headset_InstanceNumberChanged;
                _Headsetcom.IsReadyChanged -= Headset_IsReadyChanged;
                _Headsetcom.IsDirtyChanged -= Headset_IsDirtyChanged;
                _Headsetcom.MicNoiseCancellationChanged -= Headset_MicNoiseCancellationChanged;
                _Headsetcom.MicNCIncomingChanged -= Headset_MicNCIncomingChanged;
                _Headsetcom.SidetoneChanged -= Headset_SidetoneChanged;
                _Headsetcom.BusyLightChanged -= Headset_BusyLightChanged;
                _Headsetcom.VoiceGuidanceChanged -= Headset_VoiceGuidanceChanged;
                _Headsetcom.SelectedPresetChanged -= Headset_SelectedPresetChanged;
                _Headsetcom.SidetoneLevelChanged -= Headset_SidetoneLevelChanged;
                _Headsetcom.MuteStatusChanged -= Headset_MuteStatusChanged;
                _Headsetcom.BandsGainChanged -= Headset_BandsGainChanged;
                _Headsetcom.AncModeChanged -= Headset_AncModeChanged;
                _Headsetcom.AncGainChanged -= Headset_AncGainChanged;
                _Headsetcom.BoomMicChanged -= Headset_BoomMicChanged;
                _Headsetcom.IsBoomMicSupportedChanged -= Headset_BoomMicSupportedChanged;
                _Headsetcom.SerialNumberChanged -= Headset_SerialNumberChanged;
                _Headsetcom.SerialNumberChanged -= Headset_SerialNumberChangedForCMA;
                _Headsetcom.WearDetectionChanged -= Headset_WearDetectionChanged;
                _Headsetcom.IsWearDetectionPauseMusicEnabledChanged -= Headset_IsWearDetectionPauseMusicEnabledChanged;
                _Headsetcom.IsWearDetectionMuteMicEnabledChanged -= Headset_IsWearDetectionMuteMicEnabledChanged;
                _Headsetcom.WearDetectionSensitivityChanged -= Headset_WearDetectionSensitivityChanged;
                _Headsetcom.WearDetectionQuickPauseChanged -= Headset_WearDetectionQuickPauseChanged;

                writelog($"Headset {obj.headsetIndex}/{obj.ModelNumber} Commodity events unregistered successfully");

                return true;
            }
            else
                writelog($"obj.headsetCommodity is not Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity for {obj.ModelNumber}");

            return false;
        }

        private async Task<bool> UnregisterEventsForHeadsetAsync(string devcieID)
        {
            writelog($"[Headset] UnregisterEventsForHeadsetAsync in ... ");
            if (devcieID == null || devcieID == string.Empty || headsetList.Count == 0)
            {
                writelog($"devcieID == string.Empty || devcieID == null || headsetList.Count == 0");

                return false;
            }

            try
            {
                // find _comdity object for this device
                writelog($"Search {devcieID} from headsetList for Unregister Events");

                bool result = false;
                foreach (var item in headsetList)
                {
                    if (item.DeviceId == devcieID)
                    {
                        result = true;
                        writelog($"Found object {item.DeviceName} from headsetList for Unregister Events");
                        result = UnregisterEventsForHeadset(item);
                        writelog($"UnregisterEventsForHeadset result is {result}");
                        result = headsetList.Remove(item);
                        writelog($"headsetList.Remove(item) result is {result}");
                        break;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                writelog($"Headset{devcieID} UnregisterEventsForHeadset Exception {e.Message}");

                return false;
            }
        }

        private void Headset_Disconnected(object sender, DisconnectedArgs e)
        {
            writelog($"[Headset] Headset_Disconnected in ... ");

            Task<bool> result = UnregisterEventsForHeadsetAsync(e.DeviceId);

            SendDTPEventToUI(CreateHeadsetEventMsg("Headset", "Headset_Disconnected", e.DeviceId));

            writelog($"[Headset] Catch event Headset_Disconnected, unregister events result is {result.Result}, current devCount is {headsetList.Count} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private async Task<bool> RegisterEventsForHeadsetAsync(string deviceID)
        {
            writelog($"[Headset] RegisterEventsForHeadsetAsync in ... ");
            if (null == _comdityHeadset || deviceID == null || deviceID == string.Empty)
            {
                writelog($"null == _comdityHeadset || deviceID == null || deviceID == string.Empty");

                return false;
            }

            try
            {
                if (_comdityHeadset is Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity _HeadsetComObj)
                {
                    writelog($"connected HeadsetComObj.DeviceItems = {_HeadsetComObj.DeviceItems.Length}");

                    int i = 0;
                    foreach (var item in _HeadsetComObj.DeviceItems)
                    {
                        writelog($"connected HeadsetComObj.DeviceItems[{i}] = {item}");
                        string jsonStr = _HeadsetComObj.DeviceItemsEx[i++].ToString();
                        writelog($"connected HeadsetComObj.DeviceItems = {jsonStr}");

                        HeadsetEventHandleObject jsonObject = JsonSerializer.Deserialize<HeadsetEventHandleObject>(jsonStr)!;

                        if (jsonObject != null && jsonObject.DeviceId == deviceID)
                        {
                            writelog($"jsonObject values: {item}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");

                            ICommodity _comdityHeadsetTmp = await _commSdk.GetCommodityAsync<IHeadsetCommodity>(new ItemId(item), CancellationToken.None);

                            if (RegisterEventsForHeadset(_comdityHeadsetTmp))
                            {
                                jsonObject.headsetIndex = item;
                                jsonObject.headsetCommodity = _comdityHeadsetTmp;
                                headsetList.Add(jsonObject);

                                writelog($"Headset {deviceID} Commodity events registered successfully");

                                return true;
                            }
                            else
                            {
                                writelog($"Headset {deviceID} Commodity events registered fail");

                                return false;
                            }
                        }
                    }
                }
                else
                {
                    writelog($"_comdityHeadset is not Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity");
                    return false;
                }
            }
            catch (Exception e)
            {
                writelog($"Headset{deviceID} RegisterEventsForHeadsetAsync Exception {e.Message}");

                return false;
            }
            writelog($"[Headset] RegisterEventsForHeadsetAsync return false ... ");
            return false;
        }

        private bool RegisterEventsForHeadset(ICommodity _comdityHeadset)
        {
            writelog($"[Headset] RegisterEventsForHeadset in ... ");
            if (null == _comdityHeadset)
            {
                writelog($"_comdityHeadset == null");

                return false;
            }

            try
            {
                if (_comdityHeadset is Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity _Headsetcom)
                {
                    _Headsetcom.FirmwareVersionChanged += Headset_FirmwareVersionChanged;
                    _Headsetcom.BatteryLevelChanged += Headset_BatteryLevelChanged;
                    _Headsetcom.BatteryStatusChanged += Headset_BatteryStatusChanged;
                    _Headsetcom.PairedHostNameChanged += Headset_PairedHostNameChanged;
                    _Headsetcom.InstanceNumberChanged += Headset_InstanceNumberChanged;
                    _Headsetcom.IsReadyChanged += Headset_IsReadyChanged;
                    _Headsetcom.IsDirtyChanged += Headset_IsDirtyChanged;
                    _Headsetcom.MicNoiseCancellationChanged += Headset_MicNoiseCancellationChanged;
                    _Headsetcom.MicNCIncomingChanged += Headset_MicNCIncomingChanged;
                    _Headsetcom.SidetoneChanged += Headset_SidetoneChanged;
                    _Headsetcom.BusyLightChanged += Headset_BusyLightChanged;
                    _Headsetcom.VoiceGuidanceChanged += Headset_VoiceGuidanceChanged;
                    _Headsetcom.SelectedPresetChanged += Headset_SelectedPresetChanged;
                    _Headsetcom.SidetoneLevelChanged += Headset_SidetoneLevelChanged;
                    _Headsetcom.MuteStatusChanged += Headset_MuteStatusChanged;
                    _Headsetcom.BandsGainChanged += Headset_BandsGainChanged;
                    _Headsetcom.AncModeChanged += Headset_AncModeChanged;
                    _Headsetcom.AncGainChanged += Headset_AncGainChanged;
                    _Headsetcom.BoomMicChanged += Headset_BoomMicChanged;
                    _Headsetcom.IsBoomMicSupportedChanged += Headset_BoomMicSupportedChanged;
                    _Headsetcom.SerialNumberChanged += Headset_SerialNumberChanged;
                    _Headsetcom.SerialNumberChanged += Headset_SerialNumberChangedForCMA;
                    _Headsetcom.WearDetectionChanged += Headset_WearDetectionChanged;
                    _Headsetcom.IsWearDetectionPauseMusicEnabledChanged += Headset_IsWearDetectionPauseMusicEnabledChanged;
                    _Headsetcom.IsWearDetectionMuteMicEnabledChanged += Headset_IsWearDetectionMuteMicEnabledChanged;
                    _Headsetcom.WearDetectionSensitivityChanged += Headset_WearDetectionSensitivityChanged;
                    _Headsetcom.WearDetectionQuickPauseChanged += Headset_WearDetectionQuickPauseChanged;

                    writelog($"Headsetcom Commodity {_Headsetcom.DeviceName}/{_Headsetcom.DeviceId}/{_Headsetcom.ModelNumber} events registered successfully");

                    return true;
                }
                else
                {
                    writelog($"_comdityHeadset is not Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity");

                    return false;
                }
            }
            catch (Exception e)
            {
                writelog($"Catch exception {e.Message} when run RegisterEventsForHeadset");

                return false;
            }
        }

        private void Headset_Connected(object sender, ConnectedArgs e)
        {
            writelog($"[Headset] Headset_Connected in ... ");

            Task<bool> result = RegisterEventsForHeadsetAsync(e.DeviceId);

            SendDTPEventToUI(CreateHeadsetEventMsg("Headset", "Headset_Connected", e.DeviceId));

            writelog($"[Headset] Catch event Headset_Connected, register events result is {result.Result} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_MuteStatusChanged(object sender, MuteStatusChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_MuteStatusChanged",
                                    e.DeviceId, $"Headset_MuteStatusChanged:{e.MuteStatus.ToString()}"));

            writelog($"[Headset] Catch event Headset_MuteStatusChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_IsReadyChanged(object sender, IsReadyChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_IsReadyChanged",
                                    e.DeviceId, $"Headset_IsReadyChanged:{e.IsReady.ToString()}"));

            writelog($"[Headset] Catch event Headset_IsReadyChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_IsDirtyChanged(object sender, IsDirtyChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_IsDirtyChanged",
                                    e.DeviceId, $"Headset_IsDirtyChanged:{e.IsDirty.ToString()}"));

            writelog($"[Headset] Catch event Headset_IsDirtyChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_FirmwareVersionChanged(object sender, FirmwareVersionChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_FirmwareVersionChanged",
                                    e.DeviceId, $"Headset_FirmwareVersionChanged:{e.FirmwareVersion}"));

            writelog($"[Headset] Catch event Headset_FirmwareVersionChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }
        private void Headset_BatteryLevelChanged(object sender, BatteryLevelChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_BatteryLevelChanged",
                                    e.DeviceId, $"Headset_BatteryLevelChanged:{e.BatteryLevel.ToString()}"));

            writelog($"[Headset] Catch event Headset_BatteryLevelChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_BatteryStatusChanged(object sender, BatteryStatusChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_BatteryStatusChanged",
                                    e.DeviceId, $"Headset_BatteryStatusChanged:{e.BatteryStatus.ToString()}"));

            writelog($"[Headset] Catch event Headset_BatteryStatusChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_PairedHostNameChanged(object sender, PairedHostNameChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_PairedHostNameChanged", e.DeviceId,
                                      "Headset_PairedHostNameIndex:" + e.Index.ToString() + ";" +
                                      "Headset_PairedHostNameNewhostName:" + e.NewhostName.ToString()));

            writelog($"[Headset] Catch event Headset_PairedHostNameChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_InstanceNumberChanged(object sender, InstanceNumberChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_InstanceNumberChanged",
                                    e.DeviceId, $"Headset_InstanceNumberChanged:{e.InstanceNumber.ToString()}"));

            writelog($"[Headset] Catch event Headset_InstanceNumberChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_MicNoiseCancellationChanged(object sender, MicNoiseCancellationChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_MicNoiseCancellationChanged",
                                    e.DeviceId, $"Headset_MicNoiseCancellationChanged:{e.MicNoiseCancellation.ToString()}"));

            writelog($"[Headset] Catch event Headset_MicNoiseCancellationChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_MicNCIncomingChanged(object sender, MicNCIncomingChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_MicNCIncomingChanged",
                                    e.DeviceId, $"Headset_MicNCIncomingChanged:{e.MicNCIncoming.ToString()}"));

            writelog($"[Headset] Catch event Headset_MicNCIncomingChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_SidetoneChanged(object sender, SidetoneChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_SidetoneChanged",
                                    e.DeviceId, $"Headset_SidetoneChanged:{e.Sidetone.ToString()}"));

            writelog($"[Headset] Catch event Headset_SidetoneChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_BusyLightChanged(object sender, BusyLightChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_BusyLightChanged",
                                    e.DeviceId, $"Headset_BusyLightChanged:{e.BusyLight.ToString()}"));

            writelog($"[Headset] Catch event Headset_BusyLightChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_VoiceGuidanceChanged(object sender, VoiceGuidanceChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_VoiceGuidanceChanged",
                                    e.DeviceId, $"Headset_VoiceGuidanceChanged:{e.VoiceGuidance.ToString()}"));

            writelog($"[Headset] Catch event Headset_VoiceGuidanceChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_SelectedPresetChanged(object sender, SelectedPresetChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_SelectedPresetChanged",
                                    e.DeviceId, $"Headset_SelectedPresetChanged:{e.SelectedPreset.ToString()}"));

            writelog($"[Headset] Catch event Headset_SelectedPresetChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_SidetoneLevelChanged(object sender, SidetoneLevelChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_SidetoneLevelChanged",
                                    e.DeviceId, $"Headset_SidetoneLevelChanged:{e.SidetoneLevel.ToString()}"));

            writelog($"[Headset] Catch event Headset_SidetoneLevelChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_AncModeChanged(object sender, AncModeChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_AncModeChanged",
                                    e.DeviceId, $"Headset_AncModeChanged:{e.AncMode.ToString()}"));

            writelog($"[Headset] Catch event Headset_AncModeChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_AncGainChanged(object sender, AncGainChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_AncGainChanged",
                                    e.DeviceId, $"Headset_AncGainChanged:{e.AncGain.ToString()}"));

            writelog($"[Headset] Catch event Headset_AncGainChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_BoomMicSupportedChanged(object sender, IsBoomMicSupportedChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_BoomMicSupportedChanged",
                                    e.DeviceId, $"Headset_BoomMicSupportedChanged:{e.IsBoomMicSupported.ToString()}"));

            writelog($"[Headset] Catch event Headset_BoomMicSupportedChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_SerialNumberChanged(object sender, SerialNumberChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_SerialNumberChanged",
                                    e.DeviceId, $"Headset_SerialNumberChanged:{e.SerialNumber}"));

            writelog($"[Headset] Catch event Headset_SerialNumberChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_SerialNumberChangedForCMA(object sender, SerialNumberChangedArgs e)
        {
            string devicename = GetHeadsetDeviceNameAsync(e.DeviceId).Result;
            if (string.IsNullOrEmpty(devicename))
                devicename = string.Empty;

            string devicefw = GetHeadsetFirmwareVersionAsync(e.DeviceId).Result;
            if (string.IsNullOrEmpty(devicefw))
                devicefw = string.Empty;

            SendDTPEventToCMA("Headset", e.DeviceId, e.SerialNumber, devicename, devicefw);
            writelog($"[Headset] Catch event Headset_SerialNumberChangedForCMA : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_BandsGainChanged(object sender, BandsGainChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_BandsGainChanged", e.DeviceId,
                                      "Headset_Band1Gain:" + e.Band1Gain.ToString() + ";" +
                                      "Headset_Band2Gain:" + e.Band2Gain.ToString() + ";" +
                                      "Headset_Band3Gain:" + e.Band3Gain.ToString() + ";" +
                                      "Headset_Band4Gain:" + e.Band4Gain.ToString() + ";" +
                                      "Headset_Band5Gain:" + e.Band5Gain.ToString()));
            writelog($"[Headset] Catch event Headset_BandsGainChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_WearDetectionChanged(object sender, WearDetectionChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_WearDetectionChanged", e.DeviceId,
                                                 $"Headset_WearDetectionChanged:{e.IsGlobalEnabled.ToString() + ";" +
                            "Headset_IsWearDetectionPauseMusicEnabledChanged:" + e.IsPauseMusicEnabled.ToString() + ";" +
                               "Headset_IsWearDetectionMuteMicEnabledChanged:" + e.IsMuteMicEnabled.ToString() + ";" +
                                    "Headset_WearDetectionSensitivityChanged:" + e.Sensitivity.ToString() + ";" +
                                     "Headset_WearDetectionQuickPauseChanged:" + e.QuickPause.ToString()}"));

            writelog($"[Headset] Catch event Headset_WearDetectionChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_WearDetectionSensitivityChanged(object sender, WearDetectionSensitivityChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_WearDetectionSensitivityChanged",
                                    e.DeviceId, $"Headset_WearDetectionSensitivityChanged:{e.WearDetectionSensitivity.ToString()}"));

            writelog($"[Headset] Catch event Headset_WearDetectionSensitivityChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_IsWearDetectionPauseMusicEnabledChanged(object sender, IsWearDetectionPauseMusicEnabledChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_IsWearDetectionPauseMusicEnabledChanged",
                                    e.DeviceId, $"Headset_IsWearDetectionPauseMusicEnabledChanged:{e.IsPauseMusicEnabled.ToString()}"));

            writelog($"[Headset] Catch event Headset_IsWearDetectionPauseMusicEnabledChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_IsWearDetectionMuteMicEnabledChanged(object sender, IsWearDetectionMuteMicEnabledChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_IsWearDetectionMuteMicEnabledChanged",
                                    e.DeviceId, $"Headset_IsWearDetectionMuteMicEnabledChanged:{e.IsWearDetectionMuteMicEnabled.ToString()}"));

            writelog($"[Headset] Catch event Headset_IsWearDetectionMuteMicEnabledChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Headset_WearDetectionQuickPauseChanged(object sender, WearDetectionQuickPauseChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_WearDetectionQuickPauseChanged",
                                    e.DeviceId, $"Headset_WearDetectionQuickPauseChanged:{e.WearDetectionQuickPause.ToString()}"));

            writelog($"[Headset] Catch event Headset_WearDetectionQuickPauseChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }
        private void Headset_BoomMicChanged(object sender, BoomMicChangedArgs e)
        {
            SendHeadsetEventToUI(CreateHeadsetEventMsg("Headset", "Headset_BoomMicChanged",
                                    e.DeviceId, $"Headset_BoomMicChanged:{e.BoomMic.ToString()}"));

            writelog($"[Headset] Catch event Headset_BoomMicChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private string CreateHeadsetEventMsg(string devType, string eventType, string devID, string eventContent = "NewValue:NoContent")
        {
            writelog($"[Headset] Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}");
            return $"HeadsetEvent_5;Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}";
        }

        public void SendHeadsetEventToUI(string sendMsg)
        {
            UpdateUINotify headsetEventNotify = new UpdateUINotify();
            headsetEventNotify.UI_Field_Name = $"{sendMsg}";
            OnUIUpdateNotify(headsetEventNotify);
        }

        public void SendDTPEventToCMA(string deviceType, string GUID, string SNnumber, string Model, string FWversion)
        {
            writelog($"[SendDTPEventToCMA] deviceType : {deviceType}, GUID : {GUID}, SNnumber : {SNnumber}, Model : {Model}, FWversion : {FWversion}");
            CMAIDEventArgs dtpEventToCMANotify = new CMAIDEventArgs();
            dtpEventToCMANotify.deviceType = deviceType;
            dtpEventToCMANotify.guid = GUID;
            dtpEventToCMANotify.snNumber = SNnumber;
            dtpEventToCMANotify.model = Model;
            dtpEventToCMANotify.fwVersion = FWversion;
            UpdateCMANotify(dtpEventToCMANotify);
        }

        #endregion Headset Event

        #region WiredAudio


        public async Task<bool> SetBassAsync(string Guid, int newValue)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                { return false; }

                if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_speakerInterfaceType, commodity, "Bass", newValue);
                    writelog(" [Speaker] SetBassAsync Success ! ");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] SetBassAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetMidRangeAsync(string Guid, int newValue)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                { return false; }

                if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_speakerInterfaceType, commodity, "MidRange", newValue);
                    writelog(" [Speaker] SetMidRangeAsync Success ! ");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] SetMidRangeAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetTrebleAsync(string Guid, int newValue)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                { return false; }

                if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_speakerInterfaceType, commodity, "Treble", newValue);
                    writelog(" [Speaker] SetTrebleAsync Success ! ");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] SetTrebleAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetProfileForSpeaker(string Guid, string newValue)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                { return false; }

                if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_speakerInterfaceType, commodity, "Profile", newValue);
                    writelog(" [Speaker] SetProfileForSpeaker Success ! ");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] SetProfileForSpeakerAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetIsWiredAudioMicMuteSoundEnableAsync(string Guid, bool newValue)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                { return false; }

                if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_speakerInterfaceType, commodity, "IsWiredAudioMicMuteSoundEnable", newValue);
                    writelog(" [Speaker] SetIsWiredAudioMicMuteSoundEnableAsync Success ! ");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] SetIsWiredAudioMicMuteSoundEnableAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetWiredAudioVolumeAdjustmentToneAsync(string Guid, int newValue)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                { return false; }

                if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_speakerInterfaceType, commodity, "WiredAudioVolumeAdjustmentTone", newValue);
                    writelog(" [Speaker] SetWiredAudioVolumeAdjustmentToneAsync Success ! ");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] SetWiredAudioVolumeAdjustmentToneAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetIsWiredAudioIMicNSEnableAsync(string Guid, bool newValue)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                { return false; }

                if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_speakerInterfaceType, commodity, "IsWiredAudioIMicNSEnable", newValue);
                    writelog(" [Speaker] SetIsWiredAudioIMicNSEnableAsync Success ! ");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] SetIsWiredAudioIMicNSEnableAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetResetToDefaultAsyncForSoundbar(string Guid, bool newValue)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                { return false; }

                if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_speakerInterfaceType, commodity, "ResetToDefault", true);
                    writelog(" [Speaker] SetResetToDefaultAsyncForSoundbar Success ! ");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] SetResetToDefaultAsyncForSoundbar failed: {ex.Message}");
                return false;
            }
        }

        /////////////////////////Get////////////////////////////////
        public async Task<string> GetWiredAudioSerialNumberAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "SerialNumber");
                    writelog($"[DTPProxyPlugin] [Speaker] GetWiredAudioSerialNumberAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [Speaker] GetWiredAudioSerialNumberAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Speaker] GetWiredAudioSerialNumberAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<JArray> GetSpeakerDeviceItemsExAsync()
        {
            try
            {
                _itemID = new ItemId(SpeakerItemID);

                if (_speakerMethodInfo != null)
                {
                    var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                    if (commodity is ICommodity)
                    {
                        var value = GetPropertyValue(_speakerInterfaceType, commodity, "DeviceItemsEx");
                        writelog($"[DTPProxyPlugin] [Speaker] GetDeviceItemsExAsync succeeded");
                        return value == null ? new JArray() : (JArray)value;
                    }
                }

                writelog($"[DTPProxyPlugin] [Speaker] GetDeviceItemsExAsync failed: Could not retrieve commodity interface");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Speaker] GetDeviceItemsExAsync failed - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetProfileNameAsync(string item)
        {
            string guid = item;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "ProfileName");
                    writelog($"[Speaker] GetProfileNameAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[Speaker] GetProfileNameAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetProfileNameAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetProfileAsync(string item)
        {
            try
            {
                if (!await GetItemIDAsync("Speaker", item))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "Profile");
                    writelog($"[Speaker] GetProfileAsync succeeded for {item}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[Speaker] GetProfileAsync failed: Could not retrieve commodity interface for {item}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetTrebleAsync failed for {item} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetBassAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "Bass");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetBassAsync: Bass is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[Speaker] GetBassAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetBassAsync: Bass is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[Speaker] GetBassAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetBassAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetMidRangeAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "MidRange");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetMidRangeAsync: MidRange is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[Speaker] GetMidRangeAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetMidRangeAsync: MidRange is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[Speaker] GetMidRangeAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetMidRangeAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetTrebleAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "Treble");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetTrebleAsync: Treble is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[Speaker] GetTrebleAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetTrebleAsync: Treble is not an integer for {guid}");
                        return -1;
                    }
                }

                writelog($"[Speaker] GetTrebleAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetTrebleAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetIsWiredAudioMicMuteSoundEnableAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsWiredAudioMicMuteSoundEnable");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetIsWiredAudioMicMuteSoundEnableAsync: IsWiredAudioMicMuteSoundEnable is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetIsWiredAudioMicMuteSoundEnableAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetIsWiredAudioMicMuteSoundEnableAsync: IsWiredAudioMicMuteSoundEnable is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[Speaker] GetIsWiredAudioMicMuteSoundEnableAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetIsWiredAudioMicMuteSoundEnableAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetWiredAudioVolumeAdjustmentToneAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "WiredAudioVolumeAdjustmentTone");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetWiredAudioVolumeAdjustmentToneAsync: WiredAudioVolumeAdjustmentTone is null for {guid}");
                        return -1;
                    }

                    if (value is int intValue)
                    {
                        writelog($"[Speaker] GetWiredAudioVolumeAdjustmentToneAsync succeeded for {guid}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetWiredAudioVolumeAdjustmentToneAsync: WiredAudioVolumeAdjustmentTone is not an integer for {guid}");
                        return -1;
                    }

                }

                writelog($"[Speaker] GetWiredAudioVolumeAdjustmentToneAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetWiredAudioVolumeAdjustmentToneAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetIsWiredAudioIMicNSEnableAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsWiredAudioIMicNSEnable");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetIsWiredAudioIMicNSEnableAsync: IsWiredAudioIMicNSEnable is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetIsWiredAudioIMicNSEnableAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetIsWiredAudioIMicNSEnableAsync: IsWiredAudioIMicNSEnable is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[Speaker] GetIsWiredAudioIMicNSEnableAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetIsWiredAudioIMicNSEnableAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsAudioEqualizerSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                {
                    writelog(" [Speaker] Failed to retrieve guid.");
                    return false;
                }
                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsAudioEqualizerSupported");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetIsAudioEqualizerSupportedAsync: IsAudioEqualizerSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetIsAudioEqualizerSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetIsAudioEqualizerSupportedAsync: IsAudioEqualizerSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[Speaker] GetIsAudioEqualizerSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetIsAudioEqualizerSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetMuteStatusAsyncForSpeaker(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "MuteStatus");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetMuteStatusAsyncForSpeaker: MuteStatus is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetMuteStatusAsyncForSpeaker succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetMuteStatusAsyncForSpeaker: MuteStatus is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[Speaker] GetMuteStatusAsyncForSpeaker failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetMuteStatusAsyncForSpeaker failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsIMicNSSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsIMicNSSupported");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetIsIMicNSSupportedAsync: IsIMicNSSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetIsIMicNSSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetIsIMicNSSupportedAsync: IsIMicNSSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[Speaker] GetIsIMicNSSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetIsIMicNSSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> GetIsVolumeAdjustmentToneSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsVolumeAdjustmentToneSupported");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetIsVolumeAdjustmentToneSupportedAsync: IsVolumeAdjustmentToneSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetIsVolumeAdjustmentToneSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetIsVolumeAdjustmentToneSupportedAsync: IsVolumeAdjustmentToneSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[Speaker] GetIsVolumeAdjustmentToneSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetIsVolumeAdjustmentToneSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> GetIsMicMuteSoundSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsMicMuteSoundSupported");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetIsMicMuteSoundSupportedAsync: IsMicMuteSoundSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetIsMicMuteSoundSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetIsMicMuteSoundSupportedAsync: IsMicMuteSoundSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[Speaker] GetIsMicMuteSoundSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetIsMicMuteSoundSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> GetPresetProfilesAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "PresetProfiles");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetPresetProfilesAsync: PresetProfiles is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetPresetProfilesAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetPresetProfilesAsync: PresetProfiles is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[Speaker] GetPresetProfilesAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetPresetProfilesAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> GetIsBassEqualizerSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsBassEqualizerSupported");
                    if (value == null)
                    {
                        writelog($"[Speaker] GetIsBassEqualizerSupportedAsync: IsBassEqualizerSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetIsBassEqualizerSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetIsBassEqualizerSupportedAsync: IsBassEqualizerSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[Speaker] GetIsBassEqualizerSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetIsBassEqualizerSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> GetIsMidRangeEqualizerSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsMidRangeEqualizerSupported");

                    if (value == null)
                    {
                        writelog($"[Speaker] GetIsMidRangeEqualizerSupportedAsync: IsMidRangeEqualizerSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetIsMidRangeEqualizerSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetIsMidRangeEqualizerSupportedAsync: IsMidRangeEqualizerSupported is not a boolean for {guid}");
                        return false;
                    }

                }

                writelog($"[Speaker] GetIsMidRangeEqualizerSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetIsMidRangeEqualizerSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> GetIsTrebleEqualizerSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsTrebleEqualizerSupported");

                    if (value == null)
                    {
                        writelog($"[Speaker] GetIsTrebleEqualizerSupportedAsync: IsTrebleEqualizerSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[Speaker] GetIsTrebleEqualizerSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[Speaker] GetIsTrebleEqualizerSupportedAsync: IsTrebleEqualizerSupported is not a boolean for {guid}");
                        return false;
                    }

                }

                writelog($"[Speaker] GetIsTrebleEqualizerSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetIsTrebleEqualizerSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Wired Audio Event

        #region Speaker Already connected do this
        /// <summary>
        /// Already connected do this
        /// </summary>
        /// <returns></returns>
        private async Task<bool> RegisterEventsForAllSpeakerAsync()
        {
            bool result = false;
            var speakers = await GetSpeakerDevsCountAsync();
            if (speakers > 0)
            {
                writelog($"Speaker instance count: {speakers} to register");

                for (int i = 0; i < speakers; i++)
                {
                    result = await RegisterEventsForSpeakerAsync(i);

                    if (!result)
                    {
                        writelog($"[Speaker] Register Events For Speaker{i} fail, try un-register and register again");

                        result = await UnregisterEventsForSpeakerAsync(i);
                        result = await RegisterEventsForSpeakerAsync(i);
                        writelog($"[Speaker] Retry register result is {result}");
                    }
                }
            }
            else
            {
                writelog($"[Speaker] No any headset instance to register.");
                return false;
            }
            return false;
        }

        /// <summary>
        /// Already connected do this
        /// </summary>
        /// <returns></returns>
        private async Task UnregisterEventsForAllSpeakerAsync()
        {
            bool result = false;
            var speakers = await GetSpeakerDevsCountAsync();
            if (speakers > 0)
            {
                writelog($"[Speaker] instance count: {speakers} to unregister.");

                for (int i = speakers - 1; i >= 0; i--)
                {
                    result = await UnregisterEventsForSpeakerAsync(i);
                }
            }
            else
                writelog($"[Speaker] No any speaker instance to unregister.");
        }

        private async Task<bool> RegisterEventsForSpeakerAsync(int index)
        {
            if (null == _commSdk || null == _comdity || index < 0)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<ISpeakerCommodity>(new ItemId($"DellPeripheral.Speaker.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.ISpeakerCommodity _Speakercom)
                {
                    _Speakercom.FirmwareVersionChanged += Speaker_OnFirmwareVersionChanged;
                    _Speakercom.MuteStatusChanged += Speaker_OnMuteStatusChanged;
                    _Speakercom.InstanceNumberChanged += Speaker_OnInstanceNumberChanged;
                    _Speakercom.CurrentSelectedProfileChanged += Speaker_OnCurrentSelectedProfileChanged;
                    _Speakercom.IsIMicNSEnabledChanged += Speaker_OnIsIMicNSEnabledChanged;
                    _Speakercom.VolumeAdjustmentToneChanged += Speaker_OnVolumeAdjustmentToneChanged;
                    _Speakercom.IsMicMuteSoundEnabledChanged += Speaker_OnIsMicMuteSoundEnabledChanged;
                    _Speakercom.BassChanged += Speaker_OnBassChanged;
                    _Speakercom.MidRangeChanged += Speaker_OnMidRangeChanged;
                    _Speakercom.TrebleChanged += Speaker_OnTrebleChanged;

                    writelog($"Speaker{index} Commodity events registered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"[Speaker] {index} RegisterEventsForSpeaker Exception {e.Message}");

                return false;
            }

            return false;
        }

        private async Task<bool> UnregisterEventsForSpeakerAsync(int index)
        {
            if (null == _commSdk || null == _comdity || index < 0)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<ISpeakerCommodity>(new ItemId($"DellPeripheral.Speaker.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.ISpeakerCommodity _Speakercom)
                {
                    _Speakercom.FirmwareVersionChanged -= Speaker_OnFirmwareVersionChanged;
                    _Speakercom.MuteStatusChanged -= Speaker_OnMuteStatusChanged;
                    _Speakercom.InstanceNumberChanged -= Speaker_OnInstanceNumberChanged;
                    _Speakercom.CurrentSelectedProfileChanged -= Speaker_OnCurrentSelectedProfileChanged;
                    _Speakercom.IsIMicNSEnabledChanged -= Speaker_OnIsIMicNSEnabledChanged;
                    _Speakercom.VolumeAdjustmentToneChanged -= Speaker_OnVolumeAdjustmentToneChanged;
                    _Speakercom.IsMicMuteSoundEnabledChanged -= Speaker_OnIsMicMuteSoundEnabledChanged;
                    _Speakercom.BassChanged -= Speaker_OnBassChanged;
                    _Speakercom.MidRangeChanged -= Speaker_OnMidRangeChanged;
                    _Speakercom.TrebleChanged -= Speaker_OnTrebleChanged;

                    writelog($"[Speaker] Speaker{index} Commodity events unregistered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"[Speaker] Speaker{index} UnregisterEventsForSpeaker Exception {e.Message}");

                return false;
            }
            return false;
        }

        #endregion  Speaker Already connected do this

        private bool UnregisterEventsForSpeaker(SpeakerEventHandleObject obj)
        {
            if (obj.speakerCommodity is Dell.TechHub.Commodity.Peripheral.ISpeakerCommodity _Speakercom)
            {
                _Speakercom.FirmwareVersionChanged -= Speaker_OnFirmwareVersionChanged;
                _Speakercom.MuteStatusChanged -= Speaker_OnMuteStatusChanged;
                _Speakercom.InstanceNumberChanged -= Speaker_OnInstanceNumberChanged;
                _Speakercom.CurrentSelectedProfileChanged -= Speaker_OnCurrentSelectedProfileChanged;
                _Speakercom.IsIMicNSEnabledChanged -= Speaker_OnIsIMicNSEnabledChanged;
                _Speakercom.VolumeAdjustmentToneChanged -= Speaker_OnVolumeAdjustmentToneChanged;
                _Speakercom.IsMicMuteSoundEnabledChanged -= Speaker_OnIsMicMuteSoundEnabledChanged;
                _Speakercom.BassChanged -= Speaker_OnBassChanged;
                _Speakercom.MidRangeChanged -= Speaker_OnMidRangeChanged;
                _Speakercom.TrebleChanged -= Speaker_OnTrebleChanged;

                writelog($"Speaker {obj.speakerIndex}/{obj.ModelNumber} Commodity events unregistered successfully");

                return true;
            }
            else
                writelog($"obj.speakerCommodity is not Dell.TechHub.Commodity.Peripheral.ISpeakerCommodity for {obj.ModelNumber}");

            return false;
        }

        private async Task<bool> UnregisterEventsForSpeakerAsync(string devcieID)
        {
            if (devcieID == null || devcieID == string.Empty || speakerList.Count == 0)
            {
                writelog($"devcieID == string.Empty || devcieID == null || speakerList.Count == 0");

                return false;
            }

            try
            {
                // find _comdity object for this device
                writelog($"Search {devcieID} from speakerList for Unregister Events");

                bool result = false;
                foreach (var item in speakerList)
                {
                    if (item.DeviceId == devcieID)
                    {
                        result = true;
                        writelog($"Found object {item.DeviceName} from speakerList for Unregister Events");
                        result = UnregisterEventsForSpeaker(item);
                        writelog($"UnregisterEventsForSpeaker result is {result}");
                        result = speakerList.Remove(item);
                        writelog($"speakerList.Remove(item) result is {result}");
                        break;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                writelog($"Speaker{devcieID} UnregisterEventsForSpeaker Exception {e.Message}");

                return false;
            }
        }

        private async Task<bool> RegisterEventsForSpeakerAsync(string deviceID)
        {
            if (null == _comdityHeadset || deviceID == null || deviceID == string.Empty)
            {
                writelog($"null == _comditySpeaker || deviceID == null || deviceID == string.Empty");

                return false;
            }

            try
            {
                if (_comditySpeaker is Dell.TechHub.Commodity.Peripheral.ISpeakerCommodity _SpeakerComObj)
                {
                    writelog($"connected _SpeakerComObj.DeviceItems = {_SpeakerComObj.DeviceItems.Length}");

                    int i = 0;
                    foreach (var item in _SpeakerComObj.DeviceItems)
                    {
                        writelog($"connected _SpeakerComObj.DeviceItems[{i}] = {item}");
                        string jsonStr = _SpeakerComObj.DeviceItemsEx[i++].ToString();
                        writelog($"connected _SpeakerComObj.DeviceItems = {jsonStr}");

                        SpeakerEventHandleObject jsonObject = JsonSerializer.Deserialize<SpeakerEventHandleObject>(jsonStr)!;

                        if (jsonObject != null && jsonObject.DeviceId == deviceID)
                        {
                            writelog($"jsonObject values: {item}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");

                            ICommodity _comditySpeakerTmp = await _commSdk.GetCommodityAsync<ISpeakerCommodity>(new ItemId(item), CancellationToken.None);

                            if (RegisterEventsForSpeaker(_comditySpeakerTmp))
                            {
                                jsonObject.speakerIndex = item;
                                jsonObject.speakerCommodity = _comditySpeakerTmp;
                                speakerList.Add(jsonObject);

                                writelog($"Speaker {deviceID} Commodity events registered successfully");

                                return true;
                            }
                            else
                            {
                                writelog($"Speaker {deviceID} Commodity events registered fail");

                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                writelog($"Speaker{deviceID} RegisterEventsForSpeakerAsync Exception {e.Message}");

                return false;
            }

            return false;
        }

        private bool RegisterEventsForSpeaker(ICommodity _comditySpeaker)
        {
            if (null == _comditySpeaker)
            {
                writelog($"_comditySpeaker == null");

                return false;
            }

            try
            {
                if (_comditySpeaker is Dell.TechHub.Commodity.Peripheral.ISpeakerCommodity _Speakercom)
                {
                    _Speakercom.FirmwareVersionChanged += Speaker_OnFirmwareVersionChanged;
                    _Speakercom.MuteStatusChanged += Speaker_OnMuteStatusChanged;
                    _Speakercom.InstanceNumberChanged += Speaker_OnInstanceNumberChanged;
                    _Speakercom.CurrentSelectedProfileChanged += Speaker_OnCurrentSelectedProfileChanged;
                    _Speakercom.IsIMicNSEnabledChanged += Speaker_OnIsIMicNSEnabledChanged;
                    _Speakercom.VolumeAdjustmentToneChanged += Speaker_OnVolumeAdjustmentToneChanged;
                    _Speakercom.IsMicMuteSoundEnabledChanged += Speaker_OnIsMicMuteSoundEnabledChanged;
                    _Speakercom.BassChanged += Speaker_OnBassChanged;
                    _Speakercom.MidRangeChanged += Speaker_OnMidRangeChanged;
                    _Speakercom.TrebleChanged += Speaker_OnTrebleChanged;

                    writelog($"Speakercom Commodity {_Speakercom.DeviceName}/{_Speakercom.DeviceId}/{_Speakercom.ModelNumber} events registered successfully");

                    return true;
                }
                else
                {
                    writelog($"_comditySpeaker is not Dell.TechHub.Commodity.Peripheral.ISpeakerCommodity");

                    return false;
                }
            }
            catch (Exception e)
            {
                writelog($"Catch exception {e.Message} when run RegisterEventsForSpeaker");

                return false;
            }
        }

        private void Speaker_Connected(object sender, ConnectedArgs e)
        {
            Task<bool> result = RegisterEventsForSpeakerAsync(e.DeviceId);

            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_Connected", e.DeviceId));

            writelog($"Catch event Speaker_Connected, register events result is {result.Result} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Speaker_Disconnected(object sender, DisconnectedArgs e)
        {
            Task<bool> result = UnregisterEventsForSpeakerAsync(e.DeviceId);

            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_Disconnected", e.DeviceId));

            writelog($"Catch event Speaker_Disconnected, unregister events result is {result.Result}, current devCount is {speakerList.Count} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Speaker_OnFirmwareVersionChanged(object sender, FirmwareVersionChangedArgs e)
        {
            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_OnFirmwareVersionChanged",
                                    e.DeviceId, $"Speaker_OnFirmwareVersionChanged:{e.FirmwareVersion.ToString()}"));

            writelog($"[Speaker] Catch event  Speaker_OnFirmwareVersionChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Speaker_OnMuteStatusChanged(object sender, MuteStatusChangedArgs e)
        {
            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_OnMuteStatusChanged",
                                    e.DeviceId, $"Speaker_OnMuteStatusChanged:{e.MuteStatus.ToString()}"));

            writelog($"[Speaker] Catch event  Speaker_OnMuteStatusChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Speaker_OnInstanceNumberChanged(object sender, InstanceNumberChangedArgs e)
        {
            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker",
                                    e.DeviceId, $"Speaker_OnInstanceNumberChanged:{e.InstanceNumber.ToString()}"));

            writelog($"[Speaker] Catch event Speaker_OnInstanceNumberChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Speaker_OnCurrentSelectedProfileChanged(object sender, CurrentSelectedProfileChangedArgs e)
        {
            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_OnCurrentSelectedProfileChanged",
                                    e.DeviceId, $"Speaker_OnCurrentSelectedProfileChanged:{e.ProfileId.ToString()}"));

            writelog($"[Speaker] Catch event Speaker_OnCurrentSelectedProfileChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Speaker_OnIsIMicNSEnabledChanged(object sender, IsIMicNSEnabledChangedArgs e)
        {
            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_OnIsIMicNSEnabledChanged",
                                    e.DeviceId, $"Speaker_OnIsIMicNSEnabledChanged:{e.IsIMicNSEnabled}"));

            writelog($"[Speaker] Catch event Speaker_OnIsIMicNSEnabledChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }
        private void Speaker_OnVolumeAdjustmentToneChanged(object sender, VolumeAdjustmentToneChangedArgs e)
        {
            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_OnVolumeAdjustmentToneChanged",
                                    e.DeviceId, $"Speaker_OnVolumeAdjustmentToneChanged:{e.VolumeAdjustmentTone.ToString()}"));

            writelog($"[Speaker] Catch event Speaker_OnVolumeAdjustmentToneChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Speaker_OnIsMicMuteSoundEnabledChanged(object sender, IsMicMuteSoundEnabledChangedArgs e)
        {
            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_OnIsMicMuteSoundEnabledChanged",
                                    e.DeviceId, $"Speaker_OnIsMicMuteSoundEnabledChanged:{e.IsMicMuteSoundEnabled.ToString()}"));

            writelog($"[Speaker] Catch event Speaker_OnIsMicMuteSoundEnabledChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Speaker_OnBassChanged(object sender, BassChangedArgs e)
        {
            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_OnBassChanged",
                                    e.DeviceId, $"Speaker_OnBassChanged:{e.Bass.ToString()}"));

            writelog($"[Speaker] Catch event Speaker_OnBassChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Speaker_OnMidRangeChanged(object sender, MidRangeChangedArgs e)
        {
            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_OnMidRangeChanged",
                                    e.DeviceId, $"Speaker_OnMidRangeChanged:{e.MidRange.ToString()}"));

            writelog($"[Speaker] Catch event Speaker_OnMidRangeChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Speaker_OnTrebleChanged(object sender, TrebleChangedArgs e)
        {
            SendSpeakerEventToUI(CreateSpeakerEventMsg("Speaker", "Speaker_OnTrebleChanged",
                                    e.DeviceId, $"Speaker_OnTrebleChanged:{e.Treble.ToString()}"));

            writelog($"[Speaker] Catch event Speaker_OnTrebleChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private string CreateSpeakerEventMsg(string devType, string eventType, string devID, string eventContent = "NewValue:NoContent")
        {
            writelog($"[Speaker] Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}");
            return $"SpeakerEvent_5;Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}";
        }

        public void SendSpeakerEventToUI(string sendMsg)
        {
            UpdateUINotify sEventNotify = new UpdateUINotify();
            sEventNotify.UI_Field_Name = $"{sendMsg}";
            OnUIUpdateNotify(sEventNotify);
        }

        #endregion

        #region Dongle

        public async Task<string> GetFirmwareVersionAsyncForDongle(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Dongle", guid))
                {
                    writelog(" [Dongle] Failed to retrieve guid.");
                    return null;
                }
                var commodity = await GetCommodityInterfaceInstanceAsync(_dongleMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_dongleInterfaceType, commodity, "FirmwareVersion");
                    writelog($"[Dongle] GetFirmwareVersionAsyncForDongle succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[Dongle] GetFirmwareVersionAsyncForDongle failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Dongle] GetFirmwareVersionAsyncForDongle failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetConnectedDeviceInfoAsyncForDongle(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Dongle", guid))
                {
                    writelog(" [Dongle] Failed to retrieve guid.");
                    return null;
                }
                var commodity = await GetCommodityInterfaceInstanceAsync(_dongleMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_dongleInterfaceType, commodity, "ConnectedDeviceInfo");
                    writelog($"[Dongle] GetConnectedDeviceInfoAsyncForDongle succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[Dongle] GetConnectedDeviceInfoAsyncForDongle failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Dongle] GetConnectedDeviceInfoAsyncForDongle failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetDeviceIdAsyncForDongle(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Dongle", guid))
                {
                    writelog(" [Dongle] Failed to retrieve guid.");
                    return null;
                }
                var commodity = await GetCommodityInterfaceInstanceAsync(_dongleMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_dongleInterfaceType, commodity, "DeviceId");
                    writelog($"[Dongle] GetDeviceIdAsyncForDongle succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[Dongle] GetDeviceIdAsyncForDongle failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Dongle] GetDeviceIdAsyncForDongle failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetPluginIdAsyncForDongle(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Dongle", guid))
                {
                    writelog(" [Dongle] Failed to retrieve guid.");
                    return null;
                }
                var commodity = await GetCommodityInterfaceInstanceAsync(_dongleMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_dongleInterfaceType, commodity, "PluginId");
                    writelog($"[Dongle] GetPluginIdAsyncForDongle succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[Dongle] GetPluginIdAsyncForDongle failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Dongle] GetPluginIdAsyncForDongle failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<JArray> GetDeviceItemsExAsyncForDongle(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("Dongle", guid))
                {
                    writelog(" [Dongle] Failed to retrieve guid.");
                    return null;
                }
                var commodity = await GetCommodityInterfaceInstanceAsync(_dongleMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_dongleInterfaceType, commodity, "DeviceItemsEx");
                    writelog($"[Dongle] GetDeviceItemsExAsyncForDongle succeeded for {guid}");
                    return value == null ? new JArray() : (JArray)value;
                }

                writelog($"[Dongle] GetDeviceItemsExAsyncForDongle failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Dongle] GetDeviceItemsExAsyncForDongle failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }


        #endregion

        #region Dock
        public async Task<DockData> GetDockData(string guid)
        {
            try
            {
                //if (!GetItemIDAsync("Dock", guid).Result)
                if (!await GetItemIDAsync("Dock", guid))
                {
                    writelog(" [Dock] Failed to retrieve guid.");
                    return null;
                }
                var commodity = await GetCommodityInterfaceInstanceAsync(_dockMethodInfo);
                if (commodity is ICommodity)
                {
                    writelog($"[Dock] GetPropertyValue go");
                    var value = GetPropertyValue(_dockInterfaceType, commodity, "DockData");
                    writelog($"[Dock] GetPropertyValue done for {guid}");
                    writelog($"[Dock] GetPropertyValue value is null = {(value == null ? "Yes" : "No")}");
                    if (value != null && value is byte[])
                    {
                        writelog($"[Dock] GetPropertyValue value is byte[] Yes");
                        try
                        {
                            byte[] dokc_bytes = (byte[])value;
                            writelog($"[Dock] GetDockData byte is null = {(dokc_bytes == null ? "Yes" : "No")}");
                            if (dokc_bytes != null)
                            {
                                writelog($"[Dock] GetDockData dokc_bytes.Length : {dokc_bytes.Length}");
                                string textString = System.Text.Encoding.UTF8.GetString(dokc_bytes);
                                writelog($"[Dock] GetDockData textString IsNullOrEmpty = {(string.IsNullOrEmpty(textString) ? "Yes" : "No")}");
                                if (!string.IsNullOrEmpty(textString))
                                {
                                    writelog($"[Dock] GetDockData dokc_bytes to string : {textString}");
                                    try
                                    {
                                        using (JsonDocument doc = JsonDocument.Parse(textString))
                                        {
                                            JsonElement root = doc.RootElement;
                                            JsonElement payloadElement = root.GetProperty("Payload");
                                            DockData dockData = JsonSerializer.Deserialize<DockData>(payloadElement.GetRawText());
                                            writelog($"[Dock] GetDockData dockData is null = {(dockData == null ? "Yes" : "No")}");
                                            if (dockData != null)
                                            {
                                                writelog($"[Dock] GetDockData dockData.ServiceTag : {dockData.ServiceTag}");
                                                writelog($"[Dock] GetDockData dockData.PackageFirmwareVersion : {dockData.PackageFirmwareVersion}");
                                                if (dockData.MarketingName.ToUpper().StartsWith("WD19S"))
                                                {
                                                    dockData.MarketingName = $"{dockData.MarketingName}_{dockData.PowerSupplyWattage}W";
                                                }
                                                return dockData;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        writelog($"[Dock] GetDockData JsonSerializer.Deserialize Error : {ex.Message}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            writelog($"[Dock] GetDockData Dock Data Error : {ex.Message}");
                        }
                    }
                    return null;
                }

                writelog($"[Dock] GetDockData failed: Could not retrieve commodity interface for {guid}");
                return await Task.FromResult<DockData>(null);
            }
            catch (Exception ex)
            {
                writelog($"[Dock] GetDockData failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }
        public async Task<string> GetFirmwareVersionForDock(string guid)
        {
            try
            {
                //if (!GetItemIDAsync("Dock", guid).Result)
                if (!await GetItemIDAsync("Dock", guid))
                {
                    writelog(" [Dock] Failed to retrieve guid.");
                    return "";
                }
                var commodity = await GetCommodityInterfaceInstanceAsync(_dockMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_dockInterfaceType, commodity, "FirmwareVersion");

                    if (value == null)
                    {
                        writelog($"[Dock] GetFirmwareVersionForDock: FirmwareVersion is null for {guid}");
                        return "";
                    }

                    if (value is string stringValue)
                    {
                        writelog($"[Dock] GetFirmwareVersionForDock succeeded for {guid}");
                        writelog($"[Dock] GetDockServiceTagForDock succeeded for {stringValue}");
                        return stringValue;
                    }
                    else
                    {
                        writelog($"[Dock] GetFirmwareVersionForDock: FirmwareVersion is not a string for {guid}");
                        return "";
                    }
                }

                writelog($"[Dock] GetFirmwareVersionForDock failed: Could not retrieve commodity interface for {guid}");
                return "";
            }
            catch (Exception ex)
            {
                writelog($"[Dock] GetFirmwareVersionForDock failed for {guid} - Exception: {ex.Message}");
                return "";
            }
        }
        public async Task<string> GetDockServiceTagForDock(string guid)
        {
            try
            {
                //if (!GetItemIDAsync("Dock", guid).Result)
                if (!await GetItemIDAsync("Dock", guid))
                {
                    writelog(" [Dock] Failed to retrieve guid.");
                    return "";
                }
                var commodity = await GetCommodityInterfaceInstanceAsync(_dockMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_dockInterfaceType, commodity, "DockServiceTag");
                    writelog($"[Dock] GetDockServiceTagForDock succeeded for {guid}");
                    writelog($"[Dock] GetDockServiceTagForDock succeeded value is null : {(value == null ? "Yes" : "No")}");
                    if (value != null)
                    {
                        byte[] dokc_bytes = (byte[])value;
                        writelog($"[Dock] GetDockServiceTagForDock dokc_bytes.Length : {dokc_bytes.Length}");
                        string textString = System.Text.Encoding.UTF8.GetString(dokc_bytes);
                        writelog($"[Dock] GetDockServiceTagForDock dokc_bytes to string : " + textString);
                        if (!string.IsNullOrEmpty(textString))
                        {
                            try
                            {
                                using (JsonDocument doc = JsonDocument.Parse(textString))
                                {
                                    JsonElement root = doc.RootElement;
                                    string payloadElement = root.GetProperty("Payload").ToString();
                                    int temp_int = 0;
                                    if (!string.IsNullOrEmpty(payloadElement))
                                    {
                                        return payloadElement;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                writelog($"[Dock] GetDockServiceTagForDock Error : {ex.Message}");
                            }
                        }
                    }
                    return "";
                }

                writelog($"[Dock] GetDockServiceTagForDock failed: Could not retrieve commodity interface for {guid}");
                return "";
            }
            catch (Exception ex)
            {
                writelog($"[Dock] GetDockServiceTagForDock failed for {guid} - Exception: {ex.Message}");
                return "";
            }
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();

            writelog($"--- run InitializeDTPProxy at OnPluginStarting before ----");
            InitializeDTPProxy();
            writelog($"--- run InitializeDTPProxy at OnPluginStarting after ----");
            writelog("DTPProxyPlugin plugin starting");
        }

        #endregion

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e?.ChangedPlugins == null || !e.ChangedPlugins.Any())
            {
            }
            if (e.ChangedPlugins.OfType<ICommodityClientSdk>().Any())
                InitializeDTPProxy();
        }

        #region EventHandlers

        private void OnNotify(DeviceChangedEventArgs e)
        {
            if (Notify != null)
                Notify(this, e);
        }

        #endregion

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void writelog(string text,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
            log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[DTPProxyPlugin] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
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

        private Type FindCommodityInterfaceType(string commodityName)
        {
            //foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => IsAssemblyCanditate(a)))
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("DDPM.Peripheral")))
            {
                try
                {
                    var type = assembly.GetExportedTypes()
                                       .FirstOrDefault(t => t.FullName.Equals($"Dell.TechHub.Commodity.Peripheral.{commodityName}", StringComparison.OrdinalIgnoreCase));
                    //Debug.WriteLine($"\n{assembly.FullName}, {assembly.Location}");
                    if (type is not null)
                        return type;
                }
                catch (Exception ex)
                {
                    writelog($"Failed to get exported type from assembly {assembly.FullName}:{ex}");
                    //Debug.WriteLine($"\n{ex}");
                }
            }
            return null;
        }

        private async Task<ICommodity> GetCommodityInterfaceInstanceAsync(MethodInfo methodInfo)
        {
            try
            {
                writelog($" GetCommodityInterfaceInstanceAsync_itemID : {_itemID} methodInfo :{methodInfo.Name}");
                dynamic rawResult = methodInfo.Invoke(_commSdk, new object[] { _itemID, new CancellationTokenSource().Token });
                //Debug.WriteLine($"rawResult: {rawResult}");
                return rawResult is null ? null : (ICommodity)await rawResult;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"\nError handling {methodInfo.MemberType}'s {_itemID} item.\n{ex}");
                writelog($"\nError handling {methodInfo.MemberType}'s {_itemID} item.\n{ex}");
                return null;
            }
        }
        bool DTPProxyPluginReady = false;

        public Task<bool> GetDTPProxyPluginReady()
        {
            return Task.FromResult(DTPProxyPluginReady);
        }

        private void InitializeDTPProxy()
        {
            if (_commSdk != null)
            {
                writelog($"InitializeDTPProxy First check _commSdk not null, return ... ");
                return;
            }

            writelog($"InitializeDTPProxy before FindPluginByType because _commSdk null ... ");
            _commSdk = (ICommodityClientSdk)_agent.PluginManager.FindPluginByType(typeof(ICommodityClientSdk));
            //_commSdk = _agent.PluginManager.FindPluginByType<ICommodityClientSdk>(PluginResolution.Dynamic);

            try
            {
                if (_commSdk != null)
                {
                    _ = Task.Run(async () =>
                    {
                        writelog($"ICommodityClientSdk.InitializeAsync Start time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        await _commSdk.InitializeAsync(appId, new CancellationTokenSource().Token);

                        IsDTPReady = true;
                        writelog($"ICommodityClientSdk.InitializeAsync Complete time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        DTPProxyPluginSDKNotify(new UpdateDTPProxyNotify() { State = "IsDTPReady OK" });
                        writelog($"Find IGlobalPeripheralCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        _globalperipheralInterfaceType = FindCommodityInterfaceType("IGlobalPeripheralCommodity");
                        if (_globalperipheralInterfaceType != null)
                        {
                            _globalperipheralMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                          .MakeGenericMethod(_globalperipheralInterfaceType);

                            writelog($"Find IGlobalPeripheralCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }
                        else
                        {
                            writelog($"Find IGlobalPeripheralCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }

                        writelog($"Find IMouseCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        _mouseInterfaceType = FindCommodityInterfaceType("IMouseCommodity");
                        if (_mouseInterfaceType != null)
                        {
                            _mouseMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                          .MakeGenericMethod(_mouseInterfaceType);

                            writelog($"Find IMouseCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }
                        else
                        {
                            writelog($"Find IMouseCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }

                        writelog($"Find IKeyboardCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        _keyboardInterfaceType = FindCommodityInterfaceType("IKeyboardCommodity");
                        if (_keyboardInterfaceType != null)
                        {
                            _keyboardMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                            .MakeGenericMethod(_keyboardInterfaceType);
                            writelog($"Find IKeyboardCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }
                        else
                        {
                            writelog($"Find IKeyboardCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }

                        writelog($"Find IWebcamCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        _webcamInterfaceType = FindCommodityInterfaceType("IWebcamCommodity");

                        if (_webcamInterfaceType != null)
                        {
                            _webcamMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                            .MakeGenericMethod(_webcamInterfaceType);
                            writelog($"Find IWebcamCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }
                        else
                        {
                            writelog($"Find IWebcamCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }

                        writelog($"Find IPenCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        _penInterfaceType = FindCommodityInterfaceType("IPenCommodity");
                        if (_penInterfaceType != null)
                        {
                            _penMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                                        .MakeGenericMethod(_penInterfaceType);
                            writelog($"Find IPenCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }
                        else
                        {
                            writelog($"Find IPenCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }

                        writelog($"Find IHeadsetCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        _headsetInterfaceType = FindCommodityInterfaceType("IHeadsetCommodity");
                        if (_headsetInterfaceType != null)
                        {
                            _headsetMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                                        .MakeGenericMethod(_headsetInterfaceType);

                            writelog($"Find IHeadsetCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }
                        else
                        {
                            writelog($"Find IHeadsetCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }

                        writelog($"Find ISpeakerCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        _speakerInterfaceType = FindCommodityInterfaceType("ISpeakerCommodity");
                        if (_speakerInterfaceType != null)
                        {
                            _speakerMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                                        .MakeGenericMethod(_speakerInterfaceType);
                            writelog($"Find ISpeakerCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }
                        else
                        {
                            writelog($"Find ISpeakerCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }

                        writelog($"Find IDongleCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        _dongleInterfaceType = FindCommodityInterfaceType("IDongleCommodity");
                        if (_dongleInterfaceType != null)
                        {
                            _dongleMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                                        .MakeGenericMethod(_dongleInterfaceType);
                            writelog($"Find IDongleCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }
                        else
                        {
                            writelog($"Find IDongleCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }

                        writelog($"Find IDockCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        _dockInterfaceType = FindCommodityInterfaceType("IDockCommodity");
                        if (_dockInterfaceType != null)
                        {
                            _dockMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                                        .MakeGenericMethod(_dockInterfaceType);
                            writelog($"Find IDockCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }
                        else
                        {
                            writelog($"Find IDockCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                        }
                        if (GlobalDefinitions.isSupport210)
                        {
                            writelog($"Find IAiraudioCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                            _airaudioInterfaceType = FindCommodityInterfaceType("IAiraudioCommodity");
                            if (_airaudioInterfaceType != null)
                            {
                                _airaudioMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                                            .MakeGenericMethod(_airaudioInterfaceType);

                                writelog($"Find IAiraudioCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                            }
                            else
                            {
                                writelog($"Find IAiraudioCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                            }

                            writelog($"Find IRtkHubCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                            _rtkhubInterfaceType = FindCommodityInterfaceType("IRtkHubCommodity");
                            if (_rtkhubInterfaceType != null)
                            {
                                _rtkhubMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                                            .MakeGenericMethod(_rtkhubInterfaceType);

                                writelog($"Find IRtkHubCommodity found time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                            }
                            else
                            {
                                writelog($"Find IRtkHubCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                            }
                        }
                        DTPProxyPluginReady = true;
                        DTPProxyPluginSDKNotify(new UpdateDTPProxyNotify() { State = "DTPProxyPluginSDK Ready OK" });
                        _ = RegisterEventAsync();
                    });
                }
                else
                {
                    writelog($"InitializeDTPProxy After FindPluginByType, _commSdk is NULL... ERROR");
                    if (_commSdk is IFrameworkPluginConditionNotification pluginCondition)
                    {
                        pluginCondition.PluginConditionChangeHandler += OnDTPProxyPluginConditionChangeHandler;
                        GetCurrentDTPProxyPluginCondition();
                        writelog($"InitializeDTPProxy After GetCurrentDTPProxyPluginCondition ... ");
                    }
                }
            }
            catch (Exception e)
            {
                writelog($"Catch exception[{e.Message}] in InitializeDTPProxy function");
            }
        }

        private async Task RegisterEventAsync()
        {
            writelog($"Register Commodity event...");
            _comdity = await _commSdk.GetCommodityAsync<IGlobalPeripheralCommodity>(new ItemId("DellPeripheral.GlobalPeripheral"), CancellationToken.None);
            if (_comdity is Dell.TechHub.Commodity.Peripheral.IGlobalPeripheralCommodity _globalperipheralcom)
            {
                try
                {
                    _globalperipheralcom.IsAnalyticsEnabledChanged += _globalperipheralcom_IsAnalyticsEnabledChanged;
                    _globalperipheralcom.IsBatteryNotificationsEnabledChanged += _globalperipheralcom_IsBatteryNotificationsEnabledChanged;
                    ;
                    _globalperipheralcom.IsLockKeyNotificationsEnabledChanged += _globalperipheralcom_IsLockKeyNotificationsEnabledChanged;
                    _globalperipheralcom.IsMuteStatusNotificationsEnabledChanged += _globalperipheralcom_IsMuteStatusNotificationsEnabledChanged;
                    _globalperipheralcom.IsPresenceDetectionSensnorStateNotificationsEnabledChanged += _globalperipheralcom_IsPresenceDetectionSensnorStateNotificationsEnabledChanged;
                    _globalperipheralcom.IsQuickAccessMenuEnabledChanged += _globalperipheralcom_IsQuickAccessMenuEnabledChanged;
                    _globalperipheralcom.IsQuickAccessMenuOSDEnabledChanged += _globalperipheralcom_IsQuickAccessMenuOSDEnabledChanged;
                    ;
                    writelog($"GlobalPeripheral Commodity event registered");
                }
                catch (Exception e)
                {
                    writelog($"Find IGlobalPeripheralCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
            }

            _comdity = await _commSdk.GetCommodityAsync<IMouseCommodity>(new ItemId("DellPeripheral.Mouse"), CancellationToken.None);
            if (_comdity is Dell.TechHub.Commodity.Peripheral.IMouseCommodity _mousecom)
            {
                try
                {
                    _mousecom.Connected += _comdity_Connected;
                    _mousecom.Disconnected += _comdity_Disconnected;
                    writelog($"Mouse Commodity event registered");
                }
                catch (Exception e)
                {
                    writelog($"Find IMouseCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
            }

            _comdity = await _commSdk.GetCommodityAsync<IPenCommodity>(new ItemId("DellPeripheral.Pen"), CancellationToken.None);
            if (_comdity is Dell.TechHub.Commodity.Peripheral.IPenCommodity _pencom)
            {
                try
                {
                    //_pencom.KeyCaptureDataChanged += _pencom_KeyCaptureDataChanged;
                    writelog($"Pen Commodity event registered");
                }
                catch (Exception e)
                {
                    writelog($"Find IPenCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
            }

            writelog($"Register Headset Commodity event by DellPeripheral.Headset...");
            _comdityHeadset = await _commSdk.GetCommodityAsync<IHeadsetCommodity>(new ItemId("DellPeripheral.Headset"), CancellationToken.None);
            if (_comdityHeadset is Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity _headsetcom)
            {
                try
                {
                    _headsetcom.Connected += Headset_Connected;
                    _headsetcom.Disconnected += Headset_Disconnected;
                    writelog($"Headset Commodity event registered, connected _headsetcom.DeviceItems = {_headsetcom.DeviceItems.Length}");
                    int i = 0;
                    foreach (var item in _headsetcom.DeviceItems)
                    {
                        writelog($"connected _headsetcom.DeviceItems[{i}] = {item}");
                        string jsonStr = _headsetcom.DeviceItemsEx[i++].ToString();
                        writelog($"connected _headsetcom.DeviceItems, jsonStr = {jsonStr}");

                        if (jsonStr != null && jsonStr != string.Empty)
                        {
                            HeadsetEventHandleObject jsonObject = JsonSerializer.Deserialize<HeadsetEventHandleObject>(jsonStr)!;
                            jsonObject.headsetCommodity = null;
                            jsonObject.headsetIndex = item;
                            writelog($"jsonObject values: {jsonObject.headsetIndex}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");
                            headsetList.Add(jsonObject);
                        }
                    }
                    writelog($"connected _headsetcom.DeviceItemsEx.Count = {_headsetcom.DeviceItemsEx.Count}");
                }
                catch (Exception e)
                {
                    writelog($"Find IHeadsetCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
            }
            await RegisterEventsForAllHeadsetAsync();

            writelog($"Register Speaker Commodity event by DellPeripheral.Speaker...");
            _comdity = await _commSdk.GetCommodityAsync<ISpeakerCommodity>(new ItemId("DellPeripheral.Speaker"), CancellationToken.None);
            if (_comdity is Dell.TechHub.Commodity.Peripheral.ISpeakerCommodity _speakercom)
            {
                try
                {
                    _speakercom.Connected += Speaker_Connected;
                    _speakercom.Disconnected += Speaker_Disconnected;
                    writelog($"Speaker Commodity event registered, connected _speakercom.DeviceItems = {_speakercom.DeviceItems.Length}");
                    int i = 0;
                    foreach (var item in _speakercom.DeviceItems)
                    {
                        writelog($"connected _speakercom.DeviceItems[{i}] = {item}");
                        string jsonStr = _speakercom.DeviceItemsEx[i++].ToString();
                        writelog($"connected _speakercom.DeviceItems, jsonStr = {jsonStr}");

                        if (jsonStr != null && jsonStr != string.Empty)
                        {
                            SpeakerEventHandleObject jsonObject = JsonSerializer.Deserialize<SpeakerEventHandleObject>(jsonStr)!;
                            jsonObject.speakerCommodity = null;
                            jsonObject.speakerIndex = item;
                            writelog($"jsonObject values: {jsonObject.speakerIndex}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");
                            speakerList.Add(jsonObject);
                        }
                    }
                    writelog($"connected _speakercom.DeviceItemsEx.Count = {_speakercom.DeviceItemsEx.Count}");
                }
                catch (Exception e)
                {
                    writelog($"Find ISpeakerCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
            }
            await RegisterEventsForAllSpeakerAsync();

            writelog($"Register Commodity event...");
            _comdity = await _commSdk.GetCommodityAsync<IDongleCommodity>(new ItemId("DellPeripheral.Dongle"), CancellationToken.None);
            if (_comdity is Dell.TechHub.Commodity.Peripheral.IDongleCommodity _donglecom)
            {
                try
                {
                    _donglecom.Connected += _comdity_Connected;
                    _donglecom.Disconnected += _comdity_Disconnected;
                    writelog($"Dongle Commodity event registered");
                }
                catch (Exception e)
                {
                    writelog($"Find IDongleCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
            }

            writelog($"Register Dock Commodity event...");
            _comdity = await _commSdk.GetCommodityAsync<IDockCommodity>(new ItemId("DellPeripheral.Dock"), CancellationToken.None);
            if (_comdity is Dell.TechHub.Commodity.Peripheral.IDockCommodity _Dockcom)
            {
                try
                {
                    _Dockcom.Connected += _comdity_Dock_Connected;
                    _Dockcom.Disconnected += _comdity_Dock_Disconnected;
                    writelog($"Dock Commodity event registered");
                }
                catch (Exception e)
                {
                    writelog($"Find IDockCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
            }

            //Derek 1119 for Webcam event
            writelog($"Register Webcam Commodity event by DellPeripheral.Webcam...");
            _comdityWebcam = await _commSdk.GetCommodityAsync<IWebcamCommodity>(new ItemId("DellPeripheral.Webcam"), CancellationToken.None);
            if (_comdityWebcam is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity _WebcamComConnectEvent)
            {
                try
                {
                    webcamList.Clear();
                    //PrintWebcamObjectInfo(_WebcamComConnectEvent);

                    _WebcamComConnectEvent.Connected += Webcam_Connected;
                    _WebcamComConnectEvent.Disconnected += Webcam_Disconnected;


                    writelog($"connected _WebcamComConnectEvent.DeviceItems = {_WebcamComConnectEvent.DeviceItems.Length}");
                    int i = 0;
                    foreach (var item in _WebcamComConnectEvent.DeviceItems)
                    {
                        //output:
                        //DellPeripheral.Webcam.0
                        writelog($"connected _WebcamComConnectEvent.DeviceItems[{i}] = {item}");

                        //output:
                        //"{
                        //DeviceName": "Dell Pro 24 Plus Video Conferencing Monitor",
                        //"DeviceId": "36ce653b-7a0f-4c85-97f7-aad029cceeb2",
                        //"ModelNumber": "P2424HEB"
                        //}
                        string jsonStr = _WebcamComConnectEvent.DeviceItemsEx[i++].ToString();
                        writelog($"connected _WebcamComConnectEvent.DeviceItems, jsonStr = {jsonStr}");

                        if (jsonStr != null && jsonStr != string.Empty)
                        {
                            WebcamEventHandleObject jsonObject = JsonSerializer.Deserialize<WebcamEventHandleObject>(jsonStr)!;
                            jsonObject.webcamCommodity = null;
                            jsonObject.webcamIndex = item;
                            writelog($"jsonObject values: {jsonObject.webcamIndex}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");
                            webcamList.Add(jsonObject);
                        }
                    }

                    writelog($"connected _WebcamComConnectEvent.DeviceItemsEx.Count = {_WebcamComConnectEvent.DeviceItemsEx.Count}");

                    writelog($"Webcam Commodity event(connected/disconnected) registered successfully");
                }
                catch (Exception e)
                {
                    writelog($"Webcam Commodity event(connected/disconnected) registered exception: {e.Message}");
                }
            }
            else
                writelog($"IWebcamCommodity not find");

            writelog($"Register AirAudio Commodity event by DellPeripheral.AirAudio...");
            if (GlobalDefinitions.isSupport210)
            {
                _comdityAirAudio = await _commSdk.GetCommodityAsync<IAirAudioCommodity>(new ItemId("DellPeripheral.AirAudio"), CancellationToken.None);
                if (_comdityAirAudio is Dell.TechHub.Commodity.Peripheral.IAirAudioCommodity _airaudiocom)
                {
                    try
                    {
                        _airaudiocom.Connected += AirAudio_Connected;
                        _airaudiocom.Disconnected += AirAudio_Disconnected;
                        writelog($"AirAudio Commodity event registered, connected _airaudiocom.DeviceItems = {_airaudiocom.DeviceItems.Length}");
                        int i = 0;
                        foreach (var item in _airaudiocom.DeviceItems)
                        {
                            writelog($"connected _airaudiocom.DeviceItems[{i}] = {item}");
                            string jsonStr = _airaudiocom.DeviceItemsEx[i++].ToString();
                            writelog($"connected _headsetcom.DeviceItems, jsonStr = {jsonStr}");

                            if (jsonStr != null && jsonStr != string.Empty)
                            {
                                AirAudioEventHandleObject jsonObject = JsonSerializer.Deserialize<AirAudioEventHandleObject>(jsonStr)!;
                                jsonObject.airaudioCommodity = null;
                                jsonObject.airaudioIndex = item;
                                writelog($"jsonObject values: {jsonObject.airaudioIndex}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");
                                airaudioList.Add(jsonObject);
                            }
                        }
                        writelog($"connected _airaudiocom.DeviceItemsEx.Count = {_airaudiocom.DeviceItemsEx.Count}");
                    }
                    catch (Exception e)
                    {
                        writelog($"Find IHeadsetCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                    }
                }
#if Support_210
                // RtkHub rtkhub
                _comdityRtkHub = await _commSdk.GetCommodityAsync<IRtkHubCommodity>(new ItemId("DellPeripheral.RtkHub"), CancellationToken.None);
                if (_comdityRtkHub is Dell.TechHub.Commodity.Peripheral.IRtkHubCommodity _rtkhubcom)
                {
                    try
                    {
                        _rtkhubcom.Connected += RtkHub_Connected;
                        _rtkhubcom.Disconnected += RtkHub_Disconnected;
                        writelog($"RtkHub Commodity event registered, connected _rtkhubcom.DeviceItems = {_rtkhubcom.DeviceItems.Length}");
                        int i = 0;
                        foreach (var item in _rtkhubcom.DeviceItems)
                        {
                            writelog($"connected _rtkhubcom.DeviceItems[{i}] = {item}");
                            string jsonStr = _rtkhubcom.DeviceItemsEx[i++].ToString();
                            writelog($"connected _rtkhubcom.DeviceItems, jsonStr = {jsonStr}");

                            if (jsonStr != null && jsonStr != string.Empty)
                            {
                                RtkHubEventHandleObject jsonObject = JsonSerializer.Deserialize<RtkHubEventHandleObject>(jsonStr)!;
                                jsonObject.rtkhubCommodity = null;
                                jsonObject.rtkhubIndex = item;
                                writelog($"jsonObject values: {jsonObject.rtkhubIndex}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");
                                rtkhubList.Add(jsonObject);
                            }
                        }
                        writelog($"connected _rtkhubcom.DeviceItemsEx.Count = {_rtkhubcom.DeviceItemsEx.Count}");
                    }
                    catch (Exception e)
                    {
                        writelog($"Find IRtkHubCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                    }
                }
#endif
            }
            await RegisterEventsForAllConnectedWebcamsAsync();

            await RegisterEventsForAllHeadsetAsync();
            if (GlobalDefinitions.isSupport210)
            {

                await RegisterEventsForAllAirAudioAsync();
            }
            //for test
            //await UnsubscribeDTPGlobalEventsAsync();
        }

        private List<WebcamEventHandleObject> CreateWebcamObjectListByCurrentConditon()
        {
            List<WebcamEventHandleObject> list = new List<WebcamEventHandleObject>();

            if (_comdityWebcam is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity webcamObj)
            {
                int i = 0;
                foreach (var item in webcamObj.DeviceItems)
                {
                    //output:
                    //DellPeripheral.Webcam.0
                    writelog($"connected _WebcamComConnectEvent.DeviceItems[{i}] = {item}");

                    //output:
                    //"{
                    //DeviceName": "Dell Pro 24 Plus Video Conferencing Monitor",
                    //"DeviceId": "36ce653b-7a0f-4c85-97f7-aad029cceeb2",
                    //"ModelNumber": "P2424HEB"
                    //}
                    string jsonStr = webcamObj.DeviceItemsEx[i++].ToString();
                    writelog($"connected _WebcamComConnectEvent.DeviceItems = {jsonStr}");

                    if (null != jsonStr && jsonStr != string.Empty)
                    {
                        WebcamEventHandleObject jsonObject = JsonSerializer.Deserialize<WebcamEventHandleObject>(jsonStr)!;
                        jsonObject.webcamCommodity = null;
                        jsonObject.webcamIndex = item;

                        list.Add(jsonObject);

                        writelog($"jsonObject values: {jsonObject.webcamIndex}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");
                    }
                }
            }

            return list;
        }

        private async Task<bool> UnsubscribeDTPGlobalEventsAsync()
        {
            try
            {
                if (null == _comdityWebcam)
                {
                    writelog($"UnsubscribeDTPGlobalEvents _comdityWebcam is null");

                    return false;
                }

                //test result: Derek 1219
                //2024.12.19 15:02:57.444 [30756] (00027) I ------DTPProxy: [DTPProxyPlugin]
                //UnsubscribeDTPGlobalEvents webcam global events successfully,
                //Caller Name:UnsubscribeDTPGlobalEventsAsync, Source Line 6939
                if (_comdityWebcam is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity _WebcamGlobalEvent)
                {
                    _WebcamGlobalEvent.Connected -= Webcam_Connected;
                    _WebcamGlobalEvent.Disconnected -= Webcam_Disconnected;

                    writelog($"UnsubscribeDTPGlobalEvents webcam global events successfully");

                    return true;
                }
                else
                    writelog($"UnsubscribeDTPGlobalEvents _comdityWebcam is not a IWebcamCommodity object");

                //////////////////////////////////////////////////////////////////////////////////////////////

                if (null == _comdityHeadset)
                {
                    writelog($"UnsubscribeDTPGlobalEvents _comdityHeadset is null");

                    return false;
                }

                if (_comdityHeadset is Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity _HeadsetGlobalEvent)
                {
                    _HeadsetGlobalEvent.Connected -= Headset_Connected;
                    _HeadsetGlobalEvent.Disconnected -= Headset_Disconnected;

                    writelog($"UnsubscribeDTPGlobalEvents headset global events successfully");

                    return true;
                }
                else
                    writelog($"UnsubscribeDTPGlobalEvents _comdityHeadset is not a IHeadsetCommodity object");

                //////////////////////////////////////////////////////////////////////////////////////////////

                if (null == _comditySpeaker)
                {
                    writelog($"UnsubscribeDTPGlobalEvents _comditySpeaker is null");

                    return false;
                }

                if (_comditySpeaker is Dell.TechHub.Commodity.Peripheral.ISpeakerCommodity _SpeakerGlobalEvent)
                {
                    _SpeakerGlobalEvent.Connected -= Speaker_Connected;
                    _SpeakerGlobalEvent.Disconnected -= Speaker_Disconnected;

                    writelog($"UnsubscribeDTPGlobalEvents Speaker global events successfully");

                    return true;
                }
                else
                    writelog($"UnsubscribeDTPGlobalEvents _comditySpeaker is not a ISpeakerCommodity object");

                return false;
            }
            catch (Exception e)
            {
                writelog($"UnsubscribeDTPGlobalEvents catch exception: {e.Message}");

                return false;
            }
        }

        private async Task<int> GetHeadsetDevsCountAsync()
        {
            var headsets = await GetHeadsetDeviceItemsExAsync();
            if (headsets != null)
            {
                Trace.WriteLine("GetHeadsetDevsCountAsync ********** " + headsets.ToString() + " ********** ");
                return headsets.Count;
            }
            else
                return 0;
        }

        private async Task<int> GetSpeakerDevsCountAsync()
        {
            var speakers = await GetSpeakerDeviceItemsExAsync();
            if (speakers != null)
            {
                Trace.WriteLine("GetSpeakerDevsCountAsync ********** " + speakers.ToString() + " ********** ");
                return speakers.Count;
            }
            else
                return 0;
        }

        private async Task<int> GetWebcamDevsCountAsync()
        {
            var webcams = await GetWebcamDeviceItemsExAsync();

            return webcams.Count;
        }

        private void PrintWebcamObjectInfo(IWebcamCommodity obj)
        {
            try
            {
                writelog($"DeviceItems = {obj!.DeviceItems.Length}");
                writelog($"DeviceItemsEx = {obj!.DeviceItemsEx.Count}");

                //if (obj!.DeviceItems.Length > 0)
                //{
                //writelog($"InstanceId = {obj!.InstanceId}"); //fail
                //writelog($"InstanceNumber = {obj!.InstanceNumber}"); //fail
                //writelog($"ItemId = {obj!.ItemId}"); //fail
                //writelog($"DeviceName = {obj!.DeviceName}"); //fail
                //}
            }
            catch (Exception e)
            {

                writelog($"PrintWebcamObjectInfo catch exception: {e.Message}");
            }
        }

        private async Task RegisterEventsForAllConnectedWebcamsAsync()
        {
            var webcams = await GetWebcamDevsCountAsync();

            if (webcams > 0)
            {
                writelog($"Webcam instance count: {webcams} to register, count from save list is {webcamList.Count}");

                for (int i = 0; i < webcams; i++)
                {
                    bool result = await RegisterEventsForWebcamAsync(i);

                    if (!result)
                    {
                        writelog($"Register Events For Webcam{i} fail, try un-register and register again");

                        result = await UnregisterEventsForWebcamAsync(i);
                        result = await RegisterEventsForWebcamAsync(i);

                        writelog($"Retry register result is {result}");
                    }
                }

                //output for double check Derek 1220
                foreach (var item in webcamList)
                {
                    writelog($"ConnectedWebcamObject = {item.webcamCommodity},{item.webcamIndex},{item.DeviceName},{item.DeviceId},{item.ModelNumber}");
                }

            }
            else
            {
                writelog($"No any connected webcam device need to register.");
            }
        }

        //Derek 1220
        //private async Task UnregisterEventsForAllWebcamsAsync()
        //{
        //    var webcams = await GetWebcamDevsCountAsync();

        //    if (webcams > 0)
        //    {
        //        writelog($"Webcam instance count: {webcams} to unregister.");

        //        for (int i = webcams - 1; i >= 0; i--)
        //        {
        //            bool result = await UnregisterEventsForWebcamAsync(i);
        //        }
        //    }
        //    else
        //        writelog($"No any webcam instance to unregister.");
        //}

        private bool RegisterEventsForWebcam(ICommodity _comdityWebcam)
        {
            if (null == _comdityWebcam)
            {
                writelog($"_comdityWebcam == null");

                return false;
            }

            try
            {
                if (_comdityWebcam is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity _Webcamcom)
                {
                    //PrintWebcamObjectInfo(_Webcamcom);
                    // 2024-12-21, Elie R19 change that. (It seems to be removed from R19)
                    //_Webcamcom.ProfileManagerAdded += Webcam_ProfileManagerAdded;
                    _Webcamcom.IsMicEnumerationOnChanged += Webcam_IsMicEnumerationOnChanged;
                    _Webcamcom.CurrentSelectedProfileChanged += Webcam_CurrentSelectedProfileChanged;
                    _Webcamcom.CustomProfileAdded += Webcam_CustomProfileAdded;
                    _Webcamcom.CustomProfileRemoved += Webcam_CustomProfileRemoved;

                    _Webcamcom.PriorityChanged += Webcam_PriorityChanged;
                    _Webcamcom.IsFocusOnChanged += Webcam_IsFocusOnChanged;
                    _Webcamcom.FocusChanged += Webcam_FocusChanged;
                    _Webcamcom.PanChanged += Webcam_PanChanged;
                    _Webcamcom.TiltChanged += Webcam_TiltChanged;
                    _Webcamcom.ZoomChanged += Webcam_ZoomChanged; //QAM also use this event
                    _Webcamcom.BrightnessChanged += Webcam_BrightnessChanged;
                    _Webcamcom.ContrastChanged += Webcam_ContrastChanged;
                    _Webcamcom.AntiFlickerChanged += Webcam_AntiFlickerChanged;
                    _Webcamcom.SaturationChanged += Webcam_SaturationChanged;
                    _Webcamcom.SharpnessChanged += Webcam_SharpnessChanged;
                    _Webcamcom.IsAutoWhiteBalanceOnChanged += Webcam_IsAutoWhiteBalanceOnChanged;
                    _Webcamcom.AutoWhiteBalanceChanged += Webcam_AutoWhiteBalanceChanged;
                    _Webcamcom.IsAutoFramingTransitionOnChanged += Webcam_IsAutoFramingTransitionOnChanged;
                    _Webcamcom.IsAutoFramingOnChanged += Webcam_IsAutoFramingOnChanged;
                    _Webcamcom.AutoFramingSensitivityChanged += Webcam_AutoFramingSensitivityChanged;
                    _Webcamcom.AutoFramingFrameSizeChanged += Webcam_AutoFramingFrameSizeChanged;
                    _Webcamcom.FieldOfViewChanged += Webcam_FieldOfViewChanged;
                    //_Webcamcom.IsHDROnChanged += Webcam_IsHDROnChanged;
                    _Webcamcom.SerialNumberChanged += Webcam_SerialNumberChanged;
                    _Webcamcom.IsZoomMeetingActiveChanged += Webcam_IsZoomMeetingActiveChanged; //for QAM
                    _Webcamcom.IsZoomScreenShareActiveChanged += Webcam_IsZoomScreenShareActiveChanged; //for QAM
                    _Webcamcom.ZoomMeetingTypeChanged += Webcam_ZoomMeetingTypeChanged; //for QAM

                    _Webcamcom.WALSnoozeTimeLeftInSecondsChanged += Webcam_WALSnoozeTimeLeftInSecondsChanged;
                    _Webcamcom.Esi_IsWALLockCountdownStartedChanged += Webcam_Esi_IsWALLockCountdownStartedChanged;
                    _Webcamcom.Esi_IsCameraSensorCoveredChanged += Webcam_Esi_IsCameraSensorCoveredChanged;
                    _Webcamcom.Esi_WALLockCountdownChanged += Webcam_Esi_WALLockCountdownChanged;

                    writelog($"Webcam Commodity {_Webcamcom.DeviceName}/{_Webcamcom.DeviceId}/{_Webcamcom.ModelNumber} events registered successfully");

                    return true;
                }
                else
                {
                    writelog($"_comdityWebcam is not Dell.TechHub.Commodity.Peripheral.IWebcamCommodity");

                    return false;
                }
            }
            catch (Exception e)
            {
                writelog($"Catch exception {e.Message} when run RegisterEventsForWebcam");

                return false;
            }
        }

        private async Task<bool> RegisterEventsForWebcamAsync(int index)
        {
            if (null == _commSdk || index < 0)
            {
                writelog($"RegisterEventsForWebcamAsync --> null == _commSdk || index < 0");

                return false;
            }

            try
            {
                string registerID = $"DellPeripheral.Webcam.{index}";
                ICommodity _comdityWebcamTmp = await _commSdk.GetCommodityAsync<IWebcamCommodity>(new ItemId(registerID), CancellationToken.None);

                if (RegisterEventsForWebcam(_comdityWebcamTmp))
                {
                    writelog($"Webcam {index} Commodity events registered successfully");

                    //Derek 1220 save this _comdityWebcamTmp to list
                    foreach (var item in webcamList)
                    {
                        if (registerID == item.webcamIndex)
                            item.webcamCommodity = _comdityWebcamTmp;
                    }

                    return true;
                }
                else
                {
                    writelog($"Webcam{index} Commodity events registered fail");

                    return false;
                }

                //if (_comdityWebcamTmp is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity _Webcamcom)
                //{
                //    //PrintWebcamObjectInfo(_Webcamcom);

                //    _Webcamcom.ProfileManagerAdded += Webcam_ProfileManagerAdded;
                //    _Webcamcom.IsMicEnumerationOnChanged += Webcam_IsMicEnumerationOnChanged;
                //    _Webcamcom.CurrentSelectedProfileChanged += Webcam_CurrentSelectedProfileChanged;
                //    _Webcamcom.CustomProfileAdded += Webcam_CustomProfileAdded;
                //    _Webcamcom.CustomProfileRemoved += Webcam_CustomProfileRemoved;

                //    _Webcamcom.PriorityChanged += Webcam_PriorityChanged;
                //    _Webcamcom.IsFocusOnChanged += Webcam_IsFocusOnChanged;
                //    _Webcamcom.FocusChanged += Webcam_FocusChanged;
                //    _Webcamcom.PanChanged += Webcam_PanChanged;
                //    _Webcamcom.TiltChanged += Webcam_TiltChanged;
                //    _Webcamcom.ZoomChanged += Webcam_ZoomChanged; //QAM also use this event
                //    _Webcamcom.BrightnessChanged += Webcam_BrightnessChanged;
                //    _Webcamcom.ContrastChanged += Webcam_ContrastChanged;
                //    _Webcamcom.AntiFlickerChanged += Webcam_AntiFlickerChanged;
                //    _Webcamcom.SaturationChanged += Webcam_SaturationChanged;
                //    _Webcamcom.SharpnessChanged += Webcam_SharpnessChanged;
                //    _Webcamcom.IsAutoWhiteBalanceOnChanged += Webcam_IsAutoWhiteBalanceOnChanged;
                //    _Webcamcom.AutoWhiteBalanceChanged += Webcam_AutoWhiteBalanceChanged;
                //    _Webcamcom.IsAutoFramingTransitionOnChanged += Webcam_IsAutoFramingTransitionOnChanged;
                //    _Webcamcom.IsAutoFramingOnChanged += Webcam_IsAutoFramingOnChanged;
                //    _Webcamcom.AutoFramingSensitivityChanged += Webcam_AutoFramingSensitivityChanged;
                //    _Webcamcom.AutoFramingFrameSizeChanged += Webcam_AutoFramingFrameSizeChanged;
                //    _Webcamcom.FieldOfViewChanged += Webcam_FieldOfViewChanged;
                //    _Webcamcom.IsHDROnChanged += Webcam_IsHDROnChanged;
                //    _Webcamcom.SerialNumberChanged += Webcam_SerialNumberChanged;
                //    _Webcamcom.IsZoomMeetingActiveChanged += Webcam_IsZoomMeetingActiveChanged; //for QAM
                //    _Webcamcom.IsZoomScreenShareActiveChanged += Webcam_IsZoomScreenShareActiveChanged; //for QAM
                //    _Webcamcom.ZoomMeetingTypeChanged += Webcam_ZoomMeetingTypeChanged; //for QAM

                //    _Webcamcom.WALSnoozeTimeLeftInSecondsChanged += Webcam_WALSnoozeTimeLeftInSecondsChanged;
                //    _Webcamcom.Esi_IsWALLockCountdownStartedChanged += Webcam_Esi_IsWALLockCountdownStartedChanged;
                //    _Webcamcom.Esi_IsCameraSensorCoveredChanged += Webcam_Esi_IsCameraSensorCoveredChanged;
                //    _Webcamcom.Esi_WALLockCountdownChanged += Webcam_Esi_WALLockCountdownChanged;

                //    writelog($"Webcam{index} Commodity events registered successfully");

                //    //Derek 1220 save this _comdityWebcamTmp to list
                //    foreach (var item in webcamList)
                //    {
                //        if (registerID == item.webcamIndex)
                //            item.webcamCommodity = _comdityWebcamTmp;
                //    }

                //    return true;
                //}
            }
            catch (Exception e)
            {
                writelog($"Webcam{index} RegisterEventsForWebcam Exception {e.Message}");

                return false;
            }
        }

        private async Task<bool> RegisterEventsForWebcamAsync(string deviceID)
        {
            if (null == _comdityWebcam || deviceID == null || deviceID == string.Empty)
            {
                writelog($"null == _comdityWebcam || deviceID == null || deviceID == string.Empty");

                return false;
            }

            try
            {
                if (_comdityWebcam is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity _WebcamComObj)
                {
                    writelog($"connected _WebcamComObj.DeviceItems = {_WebcamComObj.DeviceItems.Length}");

                    int i = 0;
                    foreach (var item in _WebcamComObj.DeviceItems)
                    {
                        //output:
                        //DellPeripheral.Webcam.0
                        writelog($"connected _WebcamComObj.DeviceItems[{i}] = {item}");

                        //output:
                        //"{
                        //DeviceName": "Dell Pro 24 Plus Video Conferencing Monitor",
                        //"DeviceId": "36ce653b-7a0f-4c85-97f7-aad029cceeb2",
                        //"ModelNumber": "P2424HEB"
                        //}
                        string jsonStr = _WebcamComObj.DeviceItemsEx[i++].ToString();
                        writelog($"connected _WebcamComObj.DeviceItems = {jsonStr}");

                        WebcamEventHandleObject jsonObject = JsonSerializer.Deserialize<WebcamEventHandleObject>(jsonStr)!;

                        if (jsonObject != null && jsonObject.DeviceId == deviceID)
                        {
                            writelog($"jsonObject values: {item}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");

                            ICommodity _comdityWebcamTmp = await _commSdk.GetCommodityAsync<IWebcamCommodity>(new ItemId(item), CancellationToken.None);

                            if (RegisterEventsForWebcam(_comdityWebcamTmp))
                            {
                                jsonObject.webcamIndex = item;
                                jsonObject.webcamCommodity = _comdityWebcamTmp;
                                webcamList.Add(jsonObject);

                                writelog($"Webcam {deviceID} Commodity events registered successfully");

                                return true;
                            }
                            else
                            {
                                writelog($"Webcam {deviceID} Commodity events registered fail");

                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                writelog($"Webcam{deviceID} RegisterEventsForWebcamAsync Exception {e.Message}");

                return false;
            }

            return false;
        }

        private bool UnregisterEventsForWebcam(WebcamEventHandleObject obj)
        {
            if (obj.webcamCommodity is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity _Webcamcom)
            {
                //_Webcamcom.ProfileManagerAdded -= Webcam_ProfileManagerAdded;
                _Webcamcom.IsMicEnumerationOnChanged -= Webcam_IsMicEnumerationOnChanged;
                _Webcamcom.CurrentSelectedProfileChanged -= Webcam_CurrentSelectedProfileChanged;
                _Webcamcom.CustomProfileAdded -= Webcam_CustomProfileAdded;
                _Webcamcom.CustomProfileRemoved -= Webcam_CustomProfileRemoved;

                _Webcamcom.PriorityChanged -= Webcam_PriorityChanged;
                _Webcamcom.IsFocusOnChanged -= Webcam_IsFocusOnChanged;
                _Webcamcom.FocusChanged -= Webcam_FocusChanged;
                _Webcamcom.PanChanged -= Webcam_PanChanged;
                _Webcamcom.TiltChanged -= Webcam_TiltChanged;
                _Webcamcom.ZoomChanged -= Webcam_ZoomChanged;
                _Webcamcom.BrightnessChanged -= Webcam_BrightnessChanged;
                _Webcamcom.ContrastChanged -= Webcam_ContrastChanged;
                _Webcamcom.AntiFlickerChanged -= Webcam_AntiFlickerChanged;
                _Webcamcom.SaturationChanged -= Webcam_SaturationChanged;
                _Webcamcom.SharpnessChanged -= Webcam_SharpnessChanged;
                _Webcamcom.IsAutoWhiteBalanceOnChanged -= Webcam_IsAutoWhiteBalanceOnChanged;
                _Webcamcom.AutoWhiteBalanceChanged -= Webcam_AutoWhiteBalanceChanged;
                _Webcamcom.IsAutoFramingTransitionOnChanged -= Webcam_IsAutoFramingTransitionOnChanged;
                _Webcamcom.IsAutoFramingOnChanged -= Webcam_IsAutoFramingOnChanged;
                _Webcamcom.AutoFramingSensitivityChanged -= Webcam_AutoFramingSensitivityChanged;
                _Webcamcom.AutoFramingFrameSizeChanged -= Webcam_AutoFramingFrameSizeChanged;
                _Webcamcom.FieldOfViewChanged -= Webcam_FieldOfViewChanged;
                //_Webcamcom.IsHDROnChanged -= Webcam_IsHDROnChanged;
                _Webcamcom.SerialNumberChanged -= Webcam_SerialNumberChanged;
                _Webcamcom.IsZoomMeetingActiveChanged -= Webcam_IsZoomMeetingActiveChanged;
                _Webcamcom.IsZoomScreenShareActiveChanged -= Webcam_IsZoomScreenShareActiveChanged;
                _Webcamcom.ZoomMeetingTypeChanged -= Webcam_ZoomMeetingTypeChanged; //for QAM

                _Webcamcom.WALSnoozeTimeLeftInSecondsChanged -= Webcam_WALSnoozeTimeLeftInSecondsChanged;
                _Webcamcom.Esi_IsWALLockCountdownStartedChanged -= Webcam_Esi_IsWALLockCountdownStartedChanged;
                _Webcamcom.Esi_IsCameraSensorCoveredChanged -= Webcam_Esi_IsCameraSensorCoveredChanged;
                _Webcamcom.Esi_WALLockCountdownChanged -= Webcam_Esi_WALLockCountdownChanged;

                writelog($"Webcam {obj.webcamIndex}/{obj.ModelNumber} Commodity events unregistered successfully");

                return true;
            }
            else
                writelog($"obj.webcamCommodity is not Dell.TechHub.Commodity.Peripheral.IWebcamCommodity for {obj.ModelNumber}");

            return false;
        }

        private async Task<bool> UnregisterEventsForWebcamAsync(int index)
        {
            if (null == _commSdk || index < 0 || webcamList.Count == 0)
            {
                writelog($"null == _commSdk || index < 0 || webcamList.Count == 0");

                return false;
            }

            try
            {
                // find _comdity object for this device
                string webcamID = $"DellPeripheral.Webcam.{index}";
                writelog($"Search {webcamID} from webcamList for Unregister Events");

                bool result = false;
                foreach (var item in webcamList)
                {
                    if (item.webcamIndex == webcamID)
                    {
                        result = true;
                        writelog($"Found object {item.DeviceName} from webcamList for Unregister Events");
                        result = UnregisterEventsForWebcam(item);
                        writelog($"UnregisterEventsForWebcam result is {result}");
                        result = webcamList.Remove(item);
                        writelog($"webcamList.Remove(item) result is {result}");
                        break;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                writelog($"Webcam{index} UnregisterEventsForWebcam Exception {e.Message}");

                return false;
            }
        }

        private async Task<bool> UnregisterEventsForWebcamAsync(string devcieID)
        {
            if (devcieID == null || devcieID == string.Empty || webcamList.Count == 0)
            {
                writelog($"devcieID == string.Empty || devcieID == null || webcamList.Count == 0");

                return false;
            }

            try
            {
                // find _comdity object for this device
                writelog($"Search {devcieID} from webcamList for Unregister Events");

                bool result = false;
                foreach (var item in webcamList)
                {
                    if (item.DeviceId == devcieID)
                    {
                        result = true;
                        writelog($"Found object {item.DeviceName} from webcamList for Unregister Events");
                        result = UnregisterEventsForWebcam(item);
                        writelog($"UnregisterEventsForWebcam result is {result}");
                        result = webcamList.Remove(item);
                        writelog($"webcamList.Remove(item) result is {result}");
                        break;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                writelog($"Webcam{devcieID} UnregisterEventsForWebcam Exception {e.Message}");

                return false;
            }
        }

        private async Task<bool> UnregisterEventsForAllWebcamsAsync()
        {
            if (null == _commSdk || 0 == webcamList.Count)
            {
                writelog($"null == _commSdk || 0 == webcamList.Count");

                return false;
            }

            try
            {
                bool result = false;

                foreach (var item in webcamList)
                {
                    result = UnregisterEventsForWebcam(item);
                }

                webcamList.Clear();

                return result;
            }
            catch (Exception e)
            {
                writelog($"Webcam UnregisterEventsForWebcam Exception {e.Message}");

                return false;
            }
        }

        private void OnDTPProxyPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDTPProxyPluginCondition();
        }

        private void GetCurrentDTPProxyPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_commSdk as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"PluginErrorCondition");
                        //_DisplayManagerPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"PluginRunningCondition");
                        //_DisplayManagerPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"PluginStartedCondition");
                        //_DisplayManagerPluginCondition = pluginCondition;
                    }
                    else
                    {
                        writelog($"Unknow condition.");
                    }
                }
            });
        }

        //private object GetPropertyValue(Type interfaceType, ICommodity commodity, string property)
        //{
        //    if (!IsDTPReady)
        //        return null;

        //    try
        //    {
        //        writelog($"commodity: {commodity.GetType().Name} Property: {property}");
        //        Debug.WriteLine($"commodity: {commodity.GetType().Name} Property: {property}");
        //        var obj = interfaceType.GetProperty(property).GetGetMethod().Invoke(commodity, null);
        //        Debug.WriteLine($"{obj.ToString()}");
        //        return obj;
        //    }
        //    catch (Exception ex)
        //    {
        //        writelog($"Error while Getting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
        //        Debug.WriteLine($"Error while Getting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
        //        return null;
        //    }
        //}
        private object GetPropertyValue(Type interfaceType, ICommodity commodity, string property)
        {
            if (!IsDTPReady)
            {
                writelog($"  IsDTPReady : {IsDTPReady}");
                return null;
            }

            if (interfaceType == null)
            {
                writelog($"  interfaceType == null");
                return null;
            }

            if (commodity == null)
            {
                writelog($"  commodity == null");
                return null;
            }

            if (string.IsNullOrEmpty(property))
            {
                writelog($"  property == IsNullOrEmpty");
                return null;
            }

            try
            {
                writelog($"commodity: {commodity.GetType().Name} Property: {property}");

                var obj = interfaceType.GetProperty(property).GetGetMethod().Invoke(commodity, null);

                if (obj == null)
                {
                    writelog($"obj = getMethod.Invoke(commodity, null), the obj == null");
                }
                else
                {
                    writelog($"{property}, get obj => {obj}");
                }

                return obj;
            }
            catch (Exception ex)
            {
                writelog($"Error while Getting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
                Debug.WriteLine($"Error while Getting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
                return null;
            }
        }


        private bool SetPropertyValue(Type interfaceType, ICommodity commodity, string property, object value)
        {
            if (_itemID != null)
            {
                try
                {
                    writelog($"ItemID: {_itemID}; Type: {interfaceType.Name}; property: {property}; value: {JsonConvert.SerializeObject(value)}");
                }
                catch (Exception ex)
                {
                    writelog($"SetPropertyValue got Exception: {ex.ToString()}");
                }
            }
            else
            {
                writelog($"SetPropertyValue _itemID is null (object)");
            }


            if (!IsDTPReady)
            {
                writelog($"SetPropertyValue IsDTPReady: {IsDTPReady}");
                return false;
            }

            try
            {
                interfaceType.GetProperty(property).GetSetMethod().Invoke(commodity, new[] { value });
                return true;
            }
            catch (Exception ex)
            {
                var itemId = _itemID != null ? _itemID.ToString() : "null";
                writelog($"Error while setting {interfaceType}.{property} on item \"{itemId}\".\n{ex} (object)");

                return false;
            }
        }

        private bool SetPropertyValue(Type interfaceType, ICommodity commodity, string property, byte[] value)
        {
            if (_itemID != null)
            {
                try
                {
                    Debug.WriteLine($"ItemID: {_itemID}; Type: {interfaceType.Name}; property: {property}; value: {JsonConvert.SerializeObject(value)}");
                    writelog($"ItemID: {_itemID}; Type: {interfaceType.Name}; property: {property}; value: {JsonConvert.SerializeObject(value)}");
                }
                catch (Exception ex)
                {
                    writelog($"SetPropertyValue got Exception: {ex.ToString()}");
                }
            }
            else
            {
                writelog($"SetPropertyValue _itemID is null (byte[])");
            }

            if (!IsDTPReady)
                return false;

            try
            {
                interfaceType.GetProperty(property).GetSetMethod().Invoke(commodity, new[] { value });
                return true;
            }
            catch (Exception ex)
            {
                var itemId = _itemID != null ? _itemID.ToString() : "null";
                writelog($"Error while setting {interfaceType}.{property} on item \"{itemId}\".\n{ex} -- (byte[])");
                return false;
            }
        }
        private void _comdity_Disconnected(object sender, DisconnectedArgs e)
        {
            writelog($"[DTPProxy] Disconnected Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
        }

        private void _comdity_Connected(object sender, ConnectedArgs e)
        {
            writelog($"[DTPProxy] Connected Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
        }
        //private void ZoomChanged(object sender, ZoomChangedArgs e)
        //{
        //    writelog($"[DTPProxy] ZoomChanged e : {e}");
        //    ZoomChanged_Notify?.Invoke(this, e);
        //}
        //private void ZoomMeetingTypeChanged(object sender, ZoomMeetingTypeChangedArgs e)
        //{
        //    writelog($"[DTPProxy] ZoomMeetingTypeChanged e : {e}");
        //    ZoomMeetingTypeChanged_Notify?.Invoke(this, e);
        //}
        //private void IsZoomMeetingActiveChanged(object sender, IsZoomMeetingActiveChangedArgs e)
        //{
        //    writelog($"[DTPProxy] IsZoomMeetingActiveChanged e : {e}");
        //    IsZoomMeetingActive_Notify?.Invoke(this, e);
        //}
        //private void IsZoomScreenShareActiveChanged(object sender, IsZoomScreenShareActiveChangedArgs e)
        //{
        //    writelog($"[DTPProxy] ZoomMeetingTypeChanged e : {e}");
        //    IsZoomScreenShareActive_Notify?.Invoke(this, e);
        //}
        private void _comdity_Dock_Disconnected(object sender, DisconnectedArgs e)
        {
            writelog($"Dock Disconnected Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
        }

        private void _comdity_Dock_Connected(object sender, ConnectedArgs e)
        {
            writelog($"Dock Connected Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
        }

        #region Speaker

        private void _speakercomdity_IsIMicNSEnabledChanged(object sender, IsIMicNSEnabledChangedArgs e)
        {
            Debug.WriteLine($"[Speaker] IsIMicNSEnabledChanged {e.IsIMicNSEnabled} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
            writelog($"[Speaker] IsIMicNSEnabledChanged {e.IsIMicNSEnabled} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
        }

        private void _speakercomdity_VolumeAdjustmentToneChanged(object sender, VolumeAdjustmentToneChangedArgs e)
        {
            Debug.WriteLine($"[Speaker] VolumeAdjustmentToneChanged {e.VolumeAdjustmentTone} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
            writelog($"[Speaker] VolumeAdjustmentToneChanged {e.VolumeAdjustmentTone} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
        }

        private void _speakercomdity_IsMicMuteSoundEnabledChanged(object sender, IsMicMuteSoundEnabledChangedArgs e)
        {
            Debug.WriteLine($"[Speaker] IsMicMuteSoundEnabledChanged {e.IsMicMuteSoundEnabled} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
            writelog($"[Speaker] IsMicMuteSoundEnabledChanged {e.IsMicMuteSoundEnabled} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
        }

        private void _speakercomdity_IsMuteStatusChanged(object sender, MuteStatusChangedArgs e)
        {
            Debug.WriteLine($"[Speaker] IsMuteStatusChanged {e.MuteStatus} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
            writelog($"[Speaker] IsMuteStatusChanged {e.MuteStatus} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
        }
        #endregion


        #region Webcam event
        //webcam register condition
        //1. some devices has connected before DTPPlugin init
        //   A. register events for them, and remove events when disconnected
        //2. new devices connected --> register

        private void Webcam_ZoomMeetingTypeChanged(object sender, ZoomMeetingTypeChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_ZoomMeetingTypeChanged",
                                    e.DeviceId, $"NewValue:{e.ZoomMeetingType}"));

            writelog($"Catch event Webcam_ZoomMeetingTypeChanged, NewValue:{e.ZoomMeetingType} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_SharpnessChanged(object sender, SharpnessChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_SharpnessChanged",
                                    e.DeviceId, $"NewValue:{e.Sharpness}"));

            writelog($"Catch event Webcam_SharpnessChanged, NewValue:{e.Sharpness} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_Esi_WALLockCountdownChanged(object sender, Esi_WALLockCountdownChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_Esi_WALLockCountdownChanged",
                                    e.DeviceId, $"NewValue:{e.WALLockCountdown}"));

            writelog($"Catch event _Webcamcom_Esi_WALLockCountdownChanged, NewValue:{e.WALLockCountdown}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_Esi_IsWALLockCountdownStartedChanged(object sender, Esi_IsWALLockCountdownStartedChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_Esi_IsWALLockCountdownStartedChanged",
                                    e.DeviceId, $"NewValue:{e.IsWALLockCountdownStarted}"));

            writelog($"Catch event _Webcamcom_Esi_IsWALLockCountdownStartedChanged, NewValue:{e.IsWALLockCountdownStarted}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_WALSnoozeTimeLeftInSecondsChanged(object sender, WALSnoozeTimeLeftInSecondsChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_WALSnoozeTimeLeftInSecondsChanged",
                                    e.DeviceId, $"NewValue:{e.WALSnoozeTimeLeftInSeconds}"));

            //writelog($"Catch event _Webcamcom_WALSnoozeTimeLeftInSecondsChanged NewValue:{e.WALSnoozeTimeLeftInSeconds}");
            writelog($"Catch event _Webcamcom_WALSnoozeTimeLeftInSecondsChanged, NewValue:{e.WALSnoozeTimeLeftInSeconds}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsZoomScreenShareActiveChanged(object sender, IsZoomScreenShareActiveChangedArgs e)
        {
            writelog($"Start Catch event _Webcamcom_IsZoomScreenShareActiveChanged, new value {e.IsZoomScreenShareActive}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");

            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsZoomScreenShareActiveChanged",
                                    e.DeviceId, $"NewValue:{e.IsZoomScreenShareActive}"));

            writelog($"End Catch event _Webcamcom_IsZoomScreenShareActiveChanged, new value {e.IsZoomScreenShareActive}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsZoomMeetingActiveChanged(object sender, IsZoomMeetingActiveChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsZoomMeetingActiveChanged",
                                    e.DeviceId, $"NewValue:{e.IsZoomMeetingActive}"));

            writelog($"Catch event _Webcamcom_IsZoomMeetingActiveChanged, NewValue:{e.IsZoomMeetingActive}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_SerialNumberChanged(object sender, SerialNumberChangedArgs e)
        {
            //SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_SerialNumberChanged",
            //                        e.DeviceId, $"NewValue:{e.SerialNumber}"));

            string devicename = GetWebcamNameAsync(e.DeviceId).Result;
            if (string.IsNullOrEmpty(devicename))
                devicename = string.Empty;

            string devicefw = GetWebcamFirmwareVersionAsync(e.DeviceId).Result;
            if (string.IsNullOrEmpty(devicefw))
                devicefw = string.Empty;

            writelog($"[Webcam] Catch event SerialNumberChanged, NewValue:{e.SerialNumber}: {DateTime.Now:hh.mm.ss.ffffff}");
            SendDTPEventToCMA("Webcam", e.DeviceId, e.SerialNumber, devicename, devicefw);
        }

        private async Task<string> GetWebcamNameAsync(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return ""; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "DeviceName");

                if (value == null)
                {
                    writelog("[GetWebcamNameAsync] GetPropertyValue returned null for DeviceName.");
                    return "";
                }
                else if (value is string stringValue)
                {
                    writelog($"[GetWebcamNameAsync] Successfully retrieved DeviceName: {stringValue}");
                    return stringValue;
                }
                else
                {
                    writelog("[GetWebcamNameAsync] GetPropertyValue returned a non-string value for DeviceName.");
                    return "";
                }
            }
            else
            {
                writelog($"[GetWebcamNameAsync]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return "";
            }
        }

        private async Task<string> GetWebcamFirmwareVersionAsync(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return ""; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "FirmwareVersion");

                if (value == null)
                {
                    writelog("[GetWebcamFirmwareVersionAsync] GetPropertyValue returned null for FirmwareVersion.");
                    return "";
                }
                else if (value is string stringValue)
                {
                    writelog($"[GetWebcamFirmwareVersionAsync] Successfully retrieved FirmwareVersion: {stringValue}");
                    return stringValue;
                }
                else
                {
                    writelog("[GetWebcamFirmwareVersionAsync] GetPropertyValue returned a non-string value for FirmwareVersion.");
                    return "";
                }
            }
            else
            {
                writelog($"[GetWebcamFirmwareVersionAsync]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return "";
            }
        }

        //private void Webcam_IsHDROnChanged(object sender, IsHDROnChangedArgs e)
        //{
        //    // << 250207 updated by Hess to prevent cli duplicate event
        //    //SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsHDROnChanged",
        //    //                        e.DeviceId, $"NewValue:{e.IsHDROn}"));
        //    // >> 

        //    writelog($"Catch event IsHDROnChanged, Guid: {e.DeviceId} NewValue:{e.IsHDROn}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        //}

        private void Webcam_FieldOfViewChanged(object sender, FieldOfViewChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_FieldOfViewChanged",
                                    e.DeviceId, $"NewValue:{e.FieldOfView}"));

            writelog($"Catch event _Webcamcom_FieldOfViewChanged, NewValue:{e.FieldOfView}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_AutoFramingFrameSizeChanged(object sender, AutoFramingFrameSizeChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_AutoFramingFrameSizeChanged",
                                    e.DeviceId, $"NewValue:{e.AutoFramingFrameSize}"));

            writelog($"Catch event _Webcamcom_AutoFramingFrameSizeChanged, NewValue:{e.AutoFramingFrameSize}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_AutoFramingSensitivityChanged(object sender, AutoFramingSensitivityChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_AutoFramingSensitivityChanged",
                                    e.DeviceId, $"NewValue:{e.AutoFramingSensitivity}"));

            writelog($"Catch event _Webcamcom_AutoFramingSensitivityChanged, NewValue:{e.AutoFramingSensitivity}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsAutoFramingOnChanged(object sender, IsAutoFramingOnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsAutoFramingOnChanged", e.DeviceId, $"NewValue:{e.IsAutoFramingOn}"));

            writelog($"Catch event _Webcamcom_IsAutoFramingOnChanged, Guid:{e.DeviceId} NewValue:{e.IsAutoFramingOn}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsAutoFramingTransitionOnChanged(object sender, IsAutoFramingTransitionOnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsAutoFramingTransitionOnChanged",
                                    e.DeviceId, $"NewValue:{e.IsAutoFramingTransitionOn}"));

            writelog($"Catch event _Webcamcom_IsAutoFramingTransitionOnChanged, NewValue:{e.IsAutoFramingTransitionOn}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_AutoWhiteBalanceChanged(object sender, AutoWhiteBalanceChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_AutoWhiteBalanceChanged",
                                    e.DeviceId, $"NewValue:{e.AutoWhiteBalance}"));

            writelog($"Catch event _Webcamcom_AutoWhiteBalanceChanged, NewValue:{e.AutoWhiteBalance}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsAutoWhiteBalanceOnChanged(object sender, IsAutoWhiteBalanceOnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsAutoWhiteBalanceOnChanged",
                                    e.DeviceId, $"NewValue:{e.IsAutoWhiteBalanceOn}"));

            writelog($"Catch event _Webcamcom_IsAutoWhiteBalanceOnChanged, NewValue:{e.IsAutoWhiteBalanceOn}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_SaturationChanged(object sender, SaturationChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_SaturationChanged",
                                    e.DeviceId, $"NewValue:{e.Saturation}"));

            writelog($"Catch event _Webcamcom_SaturationChanged, NewValue:{e.Saturation}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_AntiFlickerChanged(object sender, AntiFlickerChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_AntiFlickerChanged",
                                    e.DeviceId, $"NewValue:{e.AntiFlicker}"));

            writelog($"Catch event _Webcamcom_AntiFlickerChanged, NewValue:{e.AntiFlicker}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_ContrastChanged(object sender, ContrastChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_ContrastChanged",
                                    e.DeviceId, $"NewValue:{e.Contrast}"));

            writelog($"Catch event _Webcamcom_ContrastChanged, NewValue:{e.Contrast}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_BrightnessChanged(object sender, BrightnessChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_BrightnessChanged",
                                    e.DeviceId, $"NewValue:{e.Brightness}"));

            writelog($"Catch event _Webcamcom_BrightnessChanged, NewValue:{e.Brightness}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_ZoomChanged(object sender, ZoomChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_ZoomChanged",
                                    e.DeviceId, $"NewValue:{e.Zoom}"));

            writelog($"Catch event _Webcamcom_ZoomChanged, NewValue:{e.Zoom}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_TiltChanged(object sender, TiltChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_TiltChanged",
                                    e.DeviceId, $"NewValue:{e.Tilt}"));

            writelog($"Catch event _Webcamcom_TiltChanged, NewValue:{e.Tilt}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_PanChanged(object sender, PanChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_PanChanged",
                                    e.DeviceId, $"NewValue:{e.Pan}"));

            writelog($"Catch event _Webcamcom_PanChanged, NewValue:{e.Pan}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_FocusChanged(object sender, FocusChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_FocusChanged",
                                    e.DeviceId, $"NewValue:{e.Focus}"));

            writelog($"Catch event _Webcamcom_FocusChanged, NewValue:{e.Focus}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsFocusOnChanged(object sender, IsFocusOnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsFocusOnChanged",
                                    e.DeviceId, $"NewValue:{e.IsFocusOn}"));

            writelog($"Catch event _Webcamcom_IsFocusOnChanged, NewValue:{e.IsFocusOn}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_PriorityChanged(object sender, PriorityChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_PriorityChanged",
                                    e.DeviceId, $"NewValue:{e.Priority}"));

            writelog($"Catch event _Webcamcom_PriorityChanged, NewValue:{e.Priority}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_CustomProfileRemoved(object sender, CustomProfileRemovedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_CustomProfileRemoved",
                                    e.DeviceId, $"NewValue:{e.ProfileId}"));

            writelog($"Catch event _Webcamcom_CustomProfileRemoved, NewValue:{e.ProfileId}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_CustomProfileAdded(object sender, CustomProfileAddedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_CustomProfileAdded",
                                    e.DeviceId, $"NewValue:{e.ProfileId}"));

            writelog($"Catch event _Webcamcom_CustomProfileAdded, NewValue:{e.ProfileId}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_CurrentSelectedProfileChanged(object sender, CurrentSelectedProfileChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_CurrentSelectedProfileChanged",
                                    e.DeviceId, $"NewValue:{e.ProfileId}"));

            writelog($"Catch event _Webcamcom_CurrentSelectedProfileChanged, NewValue:{e.ProfileId}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsMicEnumerationOnChanged(object sender, IsMicEnumerationOnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsMicEnumerationOnChanged",
                                    e.DeviceId, $"NewValue:{e.IsMicEnumerationOn}"));

            writelog($"Catch event _Webcamcom_IsMicEnumerationOnChanged, NewValue:{e.IsMicEnumerationOn}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_ProfileManagerAdded(object sender, ProfileManagerAddedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_ProfileManagerAdded",
                                    e.DeviceId, $"NewValue:{e.ProfileMangerId}"));

            writelog($"Catch event _Webcamcom_ProfileManagerAdded, NewValue:{e.ProfileMangerId}: {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_Esi_IsCameraSensorCoveredChanged(object sender, Esi_IsCameraSensorCoveredChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_Esi_IsCameraSensorCoveredChanged",
                                    e.DeviceId, $"NewValue:{e.IsCameraSensorCovered}"));

            writelog($"Catch event _Webcamcom_Esi_IsCameraSensorCoveredChanged, NewValue:{e.IsCameraSensorCovered}: new IsCameraSensorCovered is {e.IsCameraSensorCovered} {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_Disconnected(object sender, DisconnectedArgs e)
        {
            //_ = UnregisterEventsForAllWebcamsAsync();
            //_ = UnregisterEventsForWebcamAsync(0);
            //_ = RegisterEventsForAllConnectedWebcamsAsync();
            Task<bool> result = UnregisterEventsForWebcamAsync(e.DeviceId);

            //SendDTPEventToUI($"3;Device:Webcam;Event:Disconnected;DeviceId:{e.DeviceId}");
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_Disconnected", e.DeviceId));

            writelog($"Catch event _Webcam_Disconnected, unregister events result is {result.Result}, current devCount is {webcamList.Count} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_Connected(object sender, ConnectedArgs e)
        {
            //Task<int> webcams = GetWebcamDevsCountAsync();
            //bool result = RegisterEventsForWebcamAsync(webcams.Result - 1).Result;
            Task<bool> result = RegisterEventsForWebcamAsync(e.DeviceId);

            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_Connected", e.DeviceId));

            writelog($"Catch event _Webcam_Connected, register events result is {result.Result} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private string CreateEventMsg(string devType, string eventType, string devID, string eventContent = "NewValue:NoContent")
        {
            return $"WebcamEvent_5;Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}";
        }

        private void SendDTPEventToUI(string sendMsg)
        {
            UpdateUINotify webcamEventNotify = new UpdateUINotify();
            webcamEventNotify.UI_Field_Name = $"{sendMsg}";

            OnUIUpdateNotify(webcamEventNotify);
        }


        #endregion

        #region AirAudio Get
        public async Task<HeadsetConnectionType> GetAirAudioConnectionTypeAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return HeadsetConnectionType.HeadsetConnectionTypeUnknown;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "ConnectionType");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioConnectionTypeAsync: ConnectionType is null for {guid}");
                        return default(HeadsetConnectionType);
                    }

                    if (value is HeadsetConnectionType connectionType)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioConnectionTypeAsync succeeded for {guid}");
                        return connectionType;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioConnectionTypeAsync: ConnectionType is not of type HeadsetConnectionType for {guid}");
                        return default(HeadsetConnectionType);
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioConnectionTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return HeadsetConnectionType.HeadsetConnectionTypeUnknown;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioConnectionTypeAsync failed for {guid} - Exception: {ex.Message}");
                return HeadsetConnectionType.HeadsetConnectionTypeUnknown;
            }
        }

        public async Task<JArray> GetAirAudioDeviceItemsAsync()
        {
            try
            {
                _itemID = new ItemId(AirAudioItemID);

                if (_headsetMethodInfo != null)
                {
                    var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                    if (commodity is ICommodity)
                    {
                        var value = GetPropertyValue(_airaudioInterfaceType, commodity, "DeviceItems");
                        writelog($"[DTPProxyPlugin] [Airaudio] GetAirAudioDeviceItemsAsync succeeded");
                        return value == null ? new JArray() : (JArray)value;
                    }
                }

                writelog($"[DTPProxyPlugin] [Airaudio] GetAirAudioDeviceItemsAsync failed: Could not retrieve commodity interface");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Airaudio] GetAirAudioDeviceItemsAsync failed - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioSerialNumberAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "SerialNumber");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetSerialNumberAsync succeeded for {guid}");
                    return value == null ? null : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetSerialNumberAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetSerialNumberAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioDeviceBatteryStatusAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "DeviceBatteryStatus");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceBatteryStatusAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceBatteryStatusAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceBatteryStatusAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioPairingHostName1Async(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "PairingHostName1");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingHostName1Async succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingHostName1Async failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingHostName1Async failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioPairingHostName2Async(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "PairingHostName2");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingHostName2Async succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingHostName2Async failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingHostName2Async failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioPairingHostName3Async(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "PairingHostName3");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingHostName3Async succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingHostName3Async failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingHostName3Async failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioPairingStatusNameAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "PairingStatusName");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingStatusNameAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingStatusNameAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairingStatusNameAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioParentDeviceTypeAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "ParentDeviceType");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioParentDeviceTypeAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioParentDeviceTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioParentDeviceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioModelNumberAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "ModelNumber");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioModelNumberAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioModelNumberAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioModelNumberAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioDeviceTypeAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "DeviceType");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceTypeAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioFirmwareVersionAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "FirmwareVersion");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioFirmwareVersionAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioFirmwareVersionAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioFirmwareVersionAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioPluginIdAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "PluginId");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPluginIdAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPluginIdAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPluginIdAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioDeviceIdAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "DeviceId");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceIdAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceIdAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceIdAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioDeviceNameAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "DeviceName");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceNameAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceNameAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceNameAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<DeviceInterfaceType> GetAirAudioDeviceInterfaceTypeAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return default;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "DeviceInterfaceType");

                    if (value is DeviceInterfaceType deviceInterfaceType)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceInterfaceTypeAsync succeeded for {guid} with value: {deviceInterfaceType}");
                        return deviceInterfaceType;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceInterfaceTypeAsync failed for {guid}. Value is not of type DeviceInterfaceType.");
                        return default(DeviceInterfaceType); // or handle the error as needed
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceInterfaceTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return default;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioDeviceInterfaceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return default;
            }
        }

        //// Content is same as GetAirAudioBoomMicAsync?
        //public async Task<bool> GetAirAudioIsWearDetectionAsync(string Guid)
        //{
        //    string guid = Guid;

        //    try
        //    {
        //        if (!await GetItemIDAsync("AirAudio", guid))
        //            return false;

        //        var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
        //        if (commodity is ICommodity)
        //        {
        //            var value = GetPropertyValue(_airaudioInterfaceType, commodity, "BoomMic");
        //            if (value == null)
        //            {
        //                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBoomMicAsync: BoomMic is null for {guid}");
        //                return false;
        //            }

        //            if (value is bool boolValue)
        //            {
        //                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBoomMicAsync succeeded for {guid}");
        //                return boolValue;
        //            }
        //            else
        //            {
        //                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBoomMicAsync: BoomMic is not a boolean for {guid}");
        //                return false;
        //            }
        //        }

        //        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBoomMicAsync failed: Could not retrieve commodity interface for {guid}");
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBoomMicAsync failed for {guid} - Exception: {ex.Message}");
        //        return false;
        //    }
        //}

        public async Task<bool> GetAirAudioMuteStatusAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "MuteStatus");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMuteStatusAsync: MuteStatus is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMuteStatusAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMuteStatusAsync: MuteStatus is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMuteStatusAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMuteStatusAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioBoomMicAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "BoomMic");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBoomMicAsync: BoomMic is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBoomMicAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBoomMicAsync: BoomMic is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBoomMicAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBoomMicAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsBoomMicSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsBoomMicSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBoomMicSupportedAsync: IsBoomMicSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBoomMicSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBoomMicSupportedAsync: IsBoomMicSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBoomMicSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBoomMicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioWearDetectionAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "WearDetection");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionAsync: WearDetection is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionAsync: WearDetection is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioVoiceGuidanceAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "VoiceGuidance");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioVoiceGuidanceAsync: VoiceGuidance is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioVoiceGuidanceAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioVoiceGuidanceAsync: VoiceGuidance is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioVoiceGuidanceAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioVoiceGuidanceAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioBusyLightAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "BusyLight");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBusyLightAsync: BusyLight is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBusyLightAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBusyLightAsync: BusyLight is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBusyLightAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBusyLightAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioSidetoneAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "Sidetone");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSidetoneAsync: Sidetone is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSidetoneAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSidetoneAsync: Sidetone is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSidetoneAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSidetoneAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioMicNCIncomingAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "MicNCIncoming");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMicNCIncomingAsync: MicNCIncoming is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMicNCIncomingAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMicNCIncomingAsync: MicNCIncoming is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMicNCIncomingAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMicNCIncomingAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsMicNCIncomingSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsMicNCIncomingSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsMicNCIncomingSupportedAsync: IsMicNCIncomingSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsMicNCIncomingSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsMicNCIncomingSupportedAsync: IsMicNCIncomingSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsMicNCIncomingSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsMicNCIncomingSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }



        //public async Task<bool> GetAirAudioIsWearDetectionQuickPauseSupportedAsync(string Guid)
        //{
        //    string guid = Guid;

        //    try
        //    {
        //        if (!await GetItemIDAsync("AirAudio", guid))
        //            return false;

        //        var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
        //        if (commodity is ICommodity)
        //        {
        //            var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionQuickPauseSupported");
        //            if (value == null)
        //            {
        //                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionQuickPauseSupportedAsync: IsWearDetectionQuickPauseSupported is null for {guid}");
        //                return false;
        //            }

        //            if (value is bool boolValue)
        //            {
        //                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionQuickPauseSupportedAsync succeeded for {guid}");
        //                return boolValue;
        //            }
        //            else
        //            {
        //                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionQuickPauseSupportedAsync: IsWearDetectionQuickPauseSupported is not a boolean for {guid}");
        //                return false;
        //            }
        //        }

        //        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionQuickPauseSupportedAsync failed: Could not retrieve commodity interface for {guid}");
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionQuickPauseSupportedAsync failed for {guid} - Exception: {ex.Message}");
        //        return false;
        //    }
        //}

        public async Task<bool> GetAirAudioIsWearDetectionMuteMicSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionMuteMicSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionMuteMicSupportedAsync: IsWearDetectionMuteMicSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionMuteMicSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionMuteMicSupportedAsync: IsWearDetectionMuteMicSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionMuteMicSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionMuteMicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionPauseMusicSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionPauseMusicSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionPauseMusicSupportedAsync: IsWearDetectionPauseMusicSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionPauseMusicSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionPauseMusicSupportedAsync: IsWearDetectionPauseMusicSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionPauseMusicSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionPauseMusicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionSensitivitySupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionSensitivitySupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionSensitivitySupportedAsync: IsWearDetectionSensitivitySupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionSensitivitySupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionSensitivitySupportedAsync: IsWearDetectionSensitivitySupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionSensitivitySupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionSensitivitySupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionSupportedAsync: IsWearDetectionSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionSupportedAsync: IsWearDetectionSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsANCSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsANCSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsANCSupportedAsync: IsANCSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsANCSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsANCSupportedAsync: IsANCSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsANCSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsANCSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsEqualizerSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsEqualizerSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsEqualizerSupportedAsync: IsEqualizerSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsEqualizerSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsEqualizerSupportedAsync: IsEqualizerSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsEqualizerSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsEqualizerSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsPresetsSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsPresetsSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsPresetsSupportedAsync: IsPresetsSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsPresetsSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsPresetsSupportedAsync: IsPresetsSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsPresetsSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsPresetsSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsVoiceGuidanceSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsVoiceGuidanceSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsVoiceGuidanceSupportedAsync: IsVoiceGuidanceSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsVoiceGuidanceSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsVoiceGuidanceSupportedAsync: IsVoiceGuidanceSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsVoiceGuidanceSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsVoiceGuidanceSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsBusyLightSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsBusyLightSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBusyLightSupportedAsync: IsBusyLightSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBusyLightSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBusyLightSupportedAsync: IsBusyLightSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBusyLightSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBusyLightSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsSidetoneSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsSidetoneSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsSidetoneSupportedAsync: IsSidetoneSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsSidetoneSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsSidetoneSupportedAsync: IsSidetoneSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsSidetoneSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsSidetoneSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsMicNoiseCancellationSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsMicNoiseCancellationSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsMicNoiseCancellationSupportedAsync: IsMicNoiseCancellationSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsMicNoiseCancellationSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsMicNoiseCancellationSupportedAsync: IsMicNoiseCancellationSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsMicNoiseCancellationSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsMicNoiseCancellationSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsDirtyAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsDirty");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsDirtyAsync: IsDirty is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsDirtyAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsDirtyAsync: IsDirty is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsDirtyAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsDirtyAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsReadyAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsReady");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsReadyAsync: IsReady is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsReadyAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsReadyAsync: IsReady is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsReadyAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsReadyAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionPauseMusicEnabledAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionPauseMusicEnabled");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionPauseMusicEnabledAsync: IsWearDetectionPauseMusicEnabled is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionPauseMusicEnabledAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionPauseMusicEnabledAsync: IsWearDetectionPauseMusicEnabled is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionPauseMusicEnabledAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionPauseMusicEnabledAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionMuteMicEnabledAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionMuteMicEnabled");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionMuteMicEnabledAsync: IsWearDetectionMuteMicEnabled is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionMuteMicEnabledAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionMuteMicEnabledAsync: IsWearDetectionMuteMicEnabled is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionMuteMicEnabledAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionMuteMicEnabledAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsBatteryLevelSupportedAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsBatteryLevelSupported");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBatteryLevelSupportedAsync: IsBatteryLevelSupported is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBatteryLevelSupportedAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBatteryLevelSupportedAsync: IsBatteryLevelSupported is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBatteryLevelSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsBatteryLevelSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetAirAudioWearDetectionSensitivityAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "WearDetectionSensitivity");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionSensitivityAsync failed for {guid}. WearDetectionSensitivity is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionSensitivityAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionSensitivityAsync failed for {guid}. WearDetectionSensitivity is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionSensitivityAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionSensitivityAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }



        public async Task<int> GetAirAudioAncGainAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "AncGain");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAncGainAsync failed for {guid}. AncGain is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAncGainAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAncGainAsync failed for {guid}. AncGain is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAncGainAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAncGainAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioAncModeAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "AncMode");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAncModeAsync failed for {guid}. AncMode is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAncModeAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAncModeAsync failed for {guid}. AncMode is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAncModeAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAncModeAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioBand1GainAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "Band1Gain");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand1GainAsync failed for {guid}. Band1Gain is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand1GainAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand1GainAsync failed for {guid}. Band1Gain is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand1GainAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand1GainAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioBand2GainAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "Band2Gain");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand2GainAsync failed for {guid}. Band2Gain is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand2GainAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand2GainAsync failed for {guid}. Band2Gain is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand2GainAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand2GainAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioBand3GainAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "Band3Gain");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand3GainAsync failed for {guid}. Band3Gain is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand3GainAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand3GainAsync failed for {guid}. Band3Gain is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand3GainAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand3GainAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioBand4GainAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "Band4Gain");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand4GainAsync failed for {guid}. Band4Gain is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand4GainAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand4GainAsync failed for {guid}. Band4Gain is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand4GainAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand4GainAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioBand5GainAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "Band5Gain");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand5GainAsync failed for {guid}. Band5Gain is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand5GainAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand5GainAsync failed for {guid}. Band5Gain is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand5GainAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBand5GainAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioSidetoneLevelAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "SidetoneLevel");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSidetoneLevelAsync failed for {guid}. SidetoneLevel is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSidetoneLevelAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSidetoneLevelAsync failed for {guid}. SidetoneLevel is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSidetoneLevelAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSidetoneLevelAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioSelectedPresetAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "SelectedPreset");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSelectedPresetAsync failed for {guid}. SelectedPreset is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSelectedPresetAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSelectedPresetAsync failed for {guid}. SelectedPreset is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSelectedPresetAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSelectedPresetAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioBatteryLevelAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "BatteryLevel");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelAsync failed for {guid}. BatteryLevel is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelAsync failed for {guid}. BatteryLevel is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioPairedDeviceCountAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "PairedDevice");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairedDeviceCountAsync failed for {guid}. PairedDevice is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairedDeviceCountAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairedDeviceCountAsync failed for {guid}. PairedDevice is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairedDeviceCountAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioPairedDeviceCountAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioMaxPairingSlotsAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "MaxPairingSlots");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMaxPairingSlotsAsync failed for {guid}. MaxPairingSlots is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMaxPairingSlotsAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMaxPairingSlotsAsync failed for {guid}. MaxPairingSlots is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMaxPairingSlotsAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMaxPairingSlotsAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioTotalNumberOfPairedHostNameAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "TotalNumberOfPairedHostName");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioTotalNumberOfPairedHostNameAsync failed for {guid}. TotalNumberOfPairedHostName is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioTotalNumberOfPairedHostNameAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioTotalNumberOfPairedHostNameAsync failed for {guid}. TotalNumberOfPairedHostName is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioTotalNumberOfPairedHostNameAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioTotalNumberOfPairedHostNameAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioInstanceIdAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "InstanceId");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioInstanceIdAsync failed for {guid}. InstanceId is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioInstanceIdAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioInstanceIdAsync failed for {guid}. InstanceId is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioInstanceIdAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioInstanceIdAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioInstanceNumberAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "InstanceNumber");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioInstanceNumberAsync failed for {guid}. InstanceNumber is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioInstanceNumberAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioInstanceNumberAsync failed for {guid}. InstanceNumber is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioInstanceNumberAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioInstanceNumberAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioODMIdAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "ODMId");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioODMIdAsync failed for {guid}. ODMId is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioODMIdAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioODMIdAsync failed for {guid}. ODMId is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioODMIdAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioODMIdAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<string> GetAirAudioSerialNumberCaseAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "SerialNumberCase");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSerialNumberCaseAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSerialNumberCaseAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioSerialNumberCaseAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioBatteryStatusLeftAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "BatteryStatusLeft");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryStatusLeftAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryStatusLeftAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryStatusLeftAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioBatteryStatusRightAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "BatteryStatusRight");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryStatusRightAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryStatusRightAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryStatusRightAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioBatteryStatusCaseAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "BatteryStatusCase");
                    writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryStatusCaseAsync succeeded for {guid}");
                    return value == null ? "" : (string)value;
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryStatusCaseAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryStatusCaseAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GetAirAudioIsAutoPowerOffEnabledAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsAutoPowerOffEnabled");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsAutoPowerOffEnabledAsync: IsMicNoiseCancellation is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsAutoPowerOffEnabledAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsAutoPowerOffEnabledAsync: IsMicNoiseCancellation is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsAutoPowerOffEnabledAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsAutoPowerOffEnabledAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioMicNoiseCancellationAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "MicNoiseCancellation");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMicNoiseCancellationAsync: IsMicNoiseCancellation is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMicNoiseCancellationAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMicNoiseCancellationAsync: IsMicNoiseCancellation is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMicNoiseCancellationAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMicNoiseCancellationAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetAirAudioWearDetectionQuickPauseAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "WearDetectionQuickPause");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionQuickPauseAsync failed for {guid}. WearDetectionQuickPause is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionQuickPauseAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionQuickPauseAsync failed for {guid}. WearDetectionQuickPause is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionQuickPauseAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioWearDetectionQuickPauseAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionAnswerCallsEnabledAsync(string Guid)
        {
            string guid = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionAnswerCallsEnabled");
                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionAnswerCallsEnabledAsync: IsMicNoiseCancellation is null for {guid}");
                        return false;
                    }

                    if (value is bool boolValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionAnswerCallsEnabledAsync succeeded for {guid}");
                        return boolValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionAnswerCallsEnabledAsync: IsMicNoiseCancellation is not a boolean for {guid}");
                        return false;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionAnswerCallsEnabledAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioIsWearDetectionAnswerCallsEnabledAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetAirAudioAutoPowerOffIntervalAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "AutoPowerOffInterval");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAutoPowerOffIntervalAsync failed for {guid}. WearDetectionQuickPause is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAutoPowerOffIntervalAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAutoPowerOffIntervalAsync failed for {guid}. WearDetectionQuickPause is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAutoPowerOffIntervalAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioAutoPowerOffIntervalAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioBatteryLevelLeftAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "BatteryLevelLeft");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelLeftAsync failed for {guid}. WearDetectionQuickPause is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelLeftAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelLeftAsync failed for {guid}. WearDetectionQuickPause is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelLeftAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelLeftAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioBatteryLevelRightAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "BatteryLevelRight");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelRightAsync failed for {guid}. WearDetectionQuickPause is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelRightAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelRightAsync failed for {guid}. WearDetectionQuickPause is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelRightAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelRightAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioBatteryLevelCaseAsync(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "BatteryLevelCase");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelCaseAsync failed for {guid}. WearDetectionQuickPause is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelCaseAsync succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelCaseAsync failed for {guid}. WearDetectionQuickPause is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelCaseAsync failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioBatteryLevelCaseAsync failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }

        public async Task<int> GetAirAudioMaxAllowedPariedHost(string Guid)
        {
            string guid = Guid;
            try
            {
                if (!await GetItemIDAsync("AirAudio", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_airaudioInterfaceType, commodity, "MaxAllowedPairedHost");

                    if (value == null)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMaxAllowedPariedHost failed for {guid}. WearDetectionQuickPause is null.");
                        return -99;
                    }
                    else if (value is int intValue)
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMaxAllowedPariedHost succeeded for {guid} with value: {intValue}");
                        return intValue;
                    }
                    else
                    {
                        writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMaxAllowedPariedHost failed for {guid}. WearDetectionQuickPause is not an integer.");
                        return -99;
                    }

                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMaxAllowedPariedHost failed: Could not retrieve commodity interface for {guid}");
                return -99;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetAirAudioMaxAllowedPariedHost failed for {guid} - Exception: {ex.Message}");
                return -99;
            }
        }
        #endregion

        #region AirAudio Set
        public async Task<bool> SetAirAudioMicNoiseCancellationAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "MicNoiseCancellation", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetMicNoiseCancellationAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetMicNoiseCancellationAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioSidetoneAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "Sidetone", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioSidetoneAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioSidetoneAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBusyLightAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "BusyLight", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioBusyLightAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioBusyLightAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioVoiceGuidanceAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "VoiceGuidance", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioVoiceGuidanceAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioVoiceGuidanceAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioSelectedPresetAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "SelectedPreset", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioSelectedPresetAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioSelectedPresetAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioSidetoneLevelAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "SidetoneLevel", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioSidetoneLevelAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioSidetoneLevelAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBandsGainAsync(string Guid, byte[] newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "BandsGain", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioBandsGainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioBandsGainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBand1GainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "Band1Gain", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioBand1GainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioBand1GainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBand2GainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "Band2Gain", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioBand2GainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioBand2GainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBand3GainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "Band3Gain", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioBand3GainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioBand3GainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBand4GainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "Band4Gain", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioBand4GainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioBand4GainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBand5GainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "Band5Gain", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioBand5GainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioBand5GainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioAncModeAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "AncMode", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioAncModeAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioAncModeAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioAncGainAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "AncGain", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioAncGainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioAncGainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioWearDetectionAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "WearDetection", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioWearDetectionAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioWearDetectionAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioFactoryResetAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "FactoryReset", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioFactoryResetAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioFactoryResetAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioIsBoomMicSupportedAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "IsBoomMicSupported", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioIsBoomMicSupportedAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioIsBoomMicSupportedAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioWearDetectionQuickPauseAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "WearDetectionQuickPause", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioWearDetectionQuickPauseAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioWearDetectionQuickPauseAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioWearDetectionSensitivityAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "WearDetectionSensitivity", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioWearDetectionSensitivityAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioWearDetectionSensitivityAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioMicNCIncomingAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "MicNCIncoming", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioMicNCIncomingAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioMicNCIncomingAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioUnPairAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "UnPair", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioUnPairAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioUnPairAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioIsWearDetectionPauseMusicEnabledAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionPauseMusicEnabled", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioIsWearDetectionPauseMusicEnabledAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioIsWearDetectionPauseMusicEnabledAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioIsWearDetectionMuteMicEnabledAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionMuteMicEnabled", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioIsWearDetectionMuteMicEnabledAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioIsWearDetectionMuteMicEnabledAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetFactoryResetAsyncValueForAirAudioAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "FactoryReset", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetFactoryResetAsyncValueForAirAudioAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetFactoryResetAsyncValueForAirAudioAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioIsAutoPowerOffEnabledAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "IsAutoPowerOffEnabled", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioIsAutoPowerOffEnabledAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioIsAutoPowerOffEnabledAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioIsWearDetectionAnswerCallsEnabledAsync(string Guid, bool newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "IsWearDetectionAnswerCallsEnabled", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioIsWearDetectionAnswerCallsEnabledAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioIsWearDetectionAnswerCallsEnabledAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioAutoPowerOffIntervalAsync(string Guid, int newValue)
        {
            string guidString = Guid;

            try
            {
                if (!await GetItemIDAsync("AirAudio", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_airaudioInterfaceType, commodity, "AutoPowerOffInterval", newValue);
                    writelog("[DTPProxyPlugin] [AirAudio] SetAirAudioAutoPowerOffIntervalAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    writelog($"[DTPProxyPlugin] [AirAudio] Could not retrieve the Commodity Interface {_airaudioInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] SetAirAudioAutoPowerOffIntervalAsync failed: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region AirAudio Event

        #region AirAudio Already connected do this
        /// <summary>
        /// Already connected do this
        /// </summary>
        /// <returns></returns>
        private async Task<bool> RegisterEventsForAllAirAudioAsync()
        {
            bool result = false;
            var airaudios = await GetAirAudioDevsCountAsync();
            if (airaudios > 0)
            {
                writelog($"AirAudio instance count: {airaudios} to register");

                for (int i = 0; i < airaudios; i++)
                {
                    result = await RegisterEventsForAirAudioAsync(i);

                    if (!result)
                    {
                        writelog($"[AirAudio] Register Events For AirAudio{i} fail, try un-register and register again");

                        result = await UnregisterEventsForAirAudioAsync(i);
                        result = await RegisterEventsForAirAudioAsync(i);
                        writelog($"[AirAudio] Retry register result is {result}");
                    }
                }
            }
            else
            {
                writelog($"[AirAudio] No any headset instance to register.");
                return false;
                //try to force release current --> will catch exception  1123
                //await UnregisterEventsForWebcamAsync(0);
            }
            return false;
        }

        /// <summary>
        /// Already connected do this
        /// </summary>
        /// <returns></returns>
        private async Task UnregisterEventsForAllAirAudioAsync()
        {
            bool result = false;
            var headsets = await GetAirAudioDevsCountAsync();
            if (headsets > 0)
            {
                writelog($"[Headset] instance count: {headsets} to unregister.");

                for (int i = headsets - 1; i >= 0; i--)
                {
                    result = await UnregisterEventsForAirAudioAsync(i);
                }
            }
            else
                writelog($"[Headset] No any headset instance to unregister.");
        }

        private async Task<bool> RegisterEventsForAirAudioAsync(int index)
        {
            if (null == _commSdk || null == _comdity || index < 0)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<IAirAudioCommodity>(new ItemId($"DellPeripheral.AirAudio.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.IAirAudioCommodity _AirAudiocom)
                {
                    _AirAudiocom.FirmwareVersionChanged += AirAudio_FirmwareVersionChanged;
                    //_AirAudiocom.BatteryLevelChanged += AirAudio_BatteryLevelChanged;
                    //_AirAudiocom.BatteryStatusChanged += AirAudio_BatteryStatusChanged;
                    _AirAudiocom.PairedHostNameChanged += AirAudio_PairedHostNameChanged;
                    _AirAudiocom.InstanceNumberChanged += AirAudio_InstanceNumberChanged;
                    _AirAudiocom.IsReadyChanged += AirAudio_IsReadyChanged;
                    _AirAudiocom.IsDirtyChanged += AirAudio_IsDirtyChanged;
                    _AirAudiocom.MicNoiseCancellationChanged += AirAudio_MicNoiseCancellationChanged;
                    _AirAudiocom.MicNCIncomingChanged += AirAudio_MicNCIncomingChanged;
                    _AirAudiocom.SidetoneChanged += AirAudio_SidetoneChanged;
                    _AirAudiocom.BusyLightChanged += AirAudio_BusyLightChanged;
                    _AirAudiocom.VoiceGuidanceChanged += AirAudio_VoiceGuidanceChanged;
                    _AirAudiocom.SelectedPresetChanged += AirAudio_SelectedPresetChanged;
                    _AirAudiocom.SidetoneLevelChanged += AirAudio_SidetoneLevelChanged;
                    _AirAudiocom.MuteStatusChanged += AirAudio_MuteStatusChanged;
                    _AirAudiocom.BandsGainChanged += AirAudio_BandsGainChanged;
                    _AirAudiocom.AncModeChanged += AirAudio_AncModeChanged;
                    _AirAudiocom.AncGainChanged += AirAudio_AncGainChanged;
                    _AirAudiocom.BoomMicChanged += AirAudio_BoomMicChanged;
                    _AirAudiocom.IsBoomMicSupportedChanged += AirAudio_BoomMicSupportedChanged;
                    _AirAudiocom.SerialNumberChanged += AirAudio_SerialNumberChanged;
                    //_AirAudiocom.WearDetectionChanged += AirAudio_WearDetectionChanged;
                    //_AirAudiocom.IsWearDetectionPauseMusicEnabledChanged += AirAudio_IsWearDetectionPauseMusicEnabledChanged;
                    //_AirAudiocom.IsWearDetectionMuteMicEnabledChanged += AirAudio_IsWearDetectionMuteMicEnabledChanged;
                    //_AirAudiocom.WearDetectionSensitivityChanged += AirAudio_WearDetectionSensitivityChanged;
                    //_AirAudiocom.WearDetectionQuickPauseChanged += AirAudio_WearDetectionQuickPauseChanged;

                    writelog($"AirAudio{index} Commodity events registered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"[AirAudio] {index} RegisterEventsForAirAudioAsync Exception {e.Message}");

                return false;
            }

            return false;
        }

        private async Task<bool> UnregisterEventsForAirAudioAsync(int index)
        {
            if (null == _commSdk || null == _comdity || index < 0)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<IAirAudioCommodity>(new ItemId($"DellPeripheral.AirAudio.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.IAirAudioCommodity _AirAudiocom)
                {
                    _AirAudiocom.FirmwareVersionChanged -= AirAudio_FirmwareVersionChanged;
                    //_AirAudiocom.BatteryLevelChanged -= AirAudio_BatteryLevelChanged;
                    //_AirAudiocom.BatteryStatusChanged -= AirAudio_BatteryStatusChanged;
                    _AirAudiocom.PairedHostNameChanged -= AirAudio_PairedHostNameChanged;
                    _AirAudiocom.InstanceNumberChanged -= AirAudio_InstanceNumberChanged;
                    _AirAudiocom.IsReadyChanged -= AirAudio_IsReadyChanged;
                    _AirAudiocom.IsDirtyChanged -= AirAudio_IsDirtyChanged;
                    _AirAudiocom.MicNoiseCancellationChanged -= AirAudio_MicNoiseCancellationChanged;
                    _AirAudiocom.MicNCIncomingChanged -= AirAudio_MicNCIncomingChanged;
                    _AirAudiocom.SidetoneChanged -= AirAudio_SidetoneChanged;
                    _AirAudiocom.BusyLightChanged -= AirAudio_BusyLightChanged;
                    _AirAudiocom.VoiceGuidanceChanged -= AirAudio_VoiceGuidanceChanged;
                    _AirAudiocom.SelectedPresetChanged -= AirAudio_SelectedPresetChanged;
                    _AirAudiocom.SidetoneLevelChanged -= AirAudio_SidetoneLevelChanged;
                    _AirAudiocom.MuteStatusChanged -= AirAudio_MuteStatusChanged;
                    _AirAudiocom.BandsGainChanged -= AirAudio_BandsGainChanged;
                    _AirAudiocom.AncModeChanged -= AirAudio_AncModeChanged;
                    _AirAudiocom.AncGainChanged -= AirAudio_AncGainChanged;
                    _AirAudiocom.BoomMicChanged -= AirAudio_BoomMicChanged;
                    _AirAudiocom.IsBoomMicSupportedChanged -= AirAudio_BoomMicSupportedChanged;
                    _AirAudiocom.SerialNumberChanged -= AirAudio_SerialNumberChanged;
                    //_AirAudiocom.WearDetectionChanged -= AirAudio_WearDetectionChanged;
                    //_AirAudiocom.IsWearDetectionPauseMusicEnabledChanged -= AirAudio_IsWearDetectionPauseMusicEnabledChanged;
                    //_AirAudiocom.IsWearDetectionMuteMicEnabledChanged -= AirAudio_IsWearDetectionMuteMicEnabledChanged;
                    //_AirAudiocom.WearDetectionSensitivityChanged -= AirAudio_WearDetectionSensitivityChanged;
                    //_AirAudiocom.WearDetectionQuickPauseChanged -= AirAudio_WearDetectionQuickPauseChanged;

                    writelog($"[AirAudio] AirAudio{index} Commodity events unregistered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"[AirAudio] AirAudio{index} UnregisterEventsForAirAudio Exception {e.Message}");

                return false;
            }
            return false;
        }

        #endregion  AirAudio Already connected do this

        private bool UnregisterEventsForAirAudio(AirAudioEventHandleObject obj)
        {
            if (obj.airaudioCommodity is Dell.TechHub.Commodity.Peripheral.IAirAudioCommodity _AirAudiocom)
            {
                _AirAudiocom.FirmwareVersionChanged -= AirAudio_FirmwareVersionChanged;
                //_AirAudiocom.BatteryLevelChanged -= AirAudio_BatteryLevelChanged;
                //_AirAudiocom.BatteryStatusChanged -= AirAudio_BatteryStatusChanged;
                _AirAudiocom.PairedHostNameChanged -= AirAudio_PairedHostNameChanged;
                _AirAudiocom.InstanceNumberChanged -= AirAudio_InstanceNumberChanged;
                _AirAudiocom.IsReadyChanged -= AirAudio_IsReadyChanged;
                _AirAudiocom.IsDirtyChanged -= AirAudio_IsDirtyChanged;
                _AirAudiocom.MicNoiseCancellationChanged -= AirAudio_MicNoiseCancellationChanged;
                _AirAudiocom.MicNCIncomingChanged -= AirAudio_MicNCIncomingChanged;
                _AirAudiocom.SidetoneChanged -= AirAudio_SidetoneChanged;
                _AirAudiocom.BusyLightChanged -= AirAudio_BusyLightChanged;
                _AirAudiocom.VoiceGuidanceChanged -= AirAudio_VoiceGuidanceChanged;
                _AirAudiocom.SelectedPresetChanged -= AirAudio_SelectedPresetChanged;
                _AirAudiocom.SidetoneLevelChanged -= AirAudio_SidetoneLevelChanged;
                _AirAudiocom.MuteStatusChanged -= AirAudio_MuteStatusChanged;
                _AirAudiocom.BandsGainChanged -= AirAudio_BandsGainChanged;
                _AirAudiocom.AncModeChanged -= AirAudio_AncModeChanged;
                _AirAudiocom.AncGainChanged -= AirAudio_AncGainChanged;
                _AirAudiocom.BoomMicChanged -= AirAudio_BoomMicChanged;
                _AirAudiocom.IsBoomMicSupportedChanged -= AirAudio_BoomMicSupportedChanged;
                _AirAudiocom.SerialNumberChanged -= AirAudio_SerialNumberChanged;
                //_AirAudiocom.WearDetectionChanged -= AirAudio_WearDetectionChanged;
                //_AirAudiocom.IsWearDetectionPauseMusicEnabledChanged -= AirAudio_IsWearDetectionPauseMusicEnabledChanged;
                //_AirAudiocom.IsWearDetectionMuteMicEnabledChanged -= AirAudio_IsWearDetectionMuteMicEnabledChanged;
                //_AirAudiocom.WearDetectionSensitivityChanged -= AirAudio_WearDetectionSensitivityChanged;
                //_AirAudiocom.WearDetectionQuickPauseChanged -= AirAudio_WearDetectionQuickPauseChanged;

                writelog($"AirAudio {obj.airaudioIndex}/{obj.ModelNumber} Commodity events unregistered successfully");

                return true;
            }
            else
                writelog($"obj.AirAudioCommodity is not Dell.TechHub.Commodity.Peripheral.IAirAudioCommodity for {obj.ModelNumber}");

            return false;
        }

        private async Task<bool> UnregisterEventsForAirAudioAsync(string devcieID)
        {
            if (devcieID == null || devcieID == string.Empty || airaudioList.Count == 0)
            {
                writelog($"devcieID == string.Empty || devcieID == null || headsetList.Count == 0");

                return false;
            }

            try
            {
                // find _comdity object for this device
                writelog($"Search {devcieID} from AirAudioList for Unregister Events");

                bool result = false;
                foreach (var item in airaudioList)
                {
                    if (item.DeviceId == devcieID)
                    {
                        result = true;
                        writelog($"Found object {item.DeviceName} from AirAudioList for Unregister Events");
                        result = UnregisterEventsForAirAudio(item);
                        writelog($"UnregisterEventsForAirAudio result is {result}");
                        result = airaudioList.Remove(item);
                        writelog($"AirAudioList.Remove(item) result is {result}");
                        break;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                writelog($"AirAudio {devcieID} UnregisterEventsForAirAudio Exception {e.Message}");

                return false;
            }
        }

        private void AirAudio_Disconnected(object sender, DisconnectedArgs e)
        {
            Task<bool> result = UnregisterEventsForAirAudioAsync(e.DeviceId);

            SendDTPEventToUI(CreateEventMsg("AirAudio", "AirAudio_Disconnected", e.DeviceId));

            writelog($"Catch event _AirAudio_Disconnected, unregister events result is {result.Result}, current devCount is {airaudioList.Count} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private async Task<bool> RegisterEventsForAirAudioAsync(string deviceID)
        {
            if (!GlobalDefinitions.isSupport210)
            {
                return false;
            }
            if (null == _comdityAirAudio || deviceID == null || deviceID == string.Empty)
            {
                writelog($"null == _comdityAirAudio || deviceID == null || deviceID == string.Empty");

                return false;
            }

            try
            {
                if (_comdityAirAudio is Dell.TechHub.Commodity.Peripheral.IAirAudioCommodity _AirAudioComObj)
                {
                    writelog($"connected _AirAudioComObj.DeviceItems = {_AirAudioComObj.DeviceItems.Length}");

                    int i = 0;
                    foreach (var item in _AirAudioComObj.DeviceItems)
                    {
                        writelog($"connected _AirAudioComObj.DeviceItems[{i}] = {item}");
                        string jsonStr = _AirAudioComObj.DeviceItemsEx[i++].ToString();
                        writelog($"connected _AirAudioComObj.DeviceItems = {jsonStr}");

                        AirAudioEventHandleObject jsonObject = JsonSerializer.Deserialize<AirAudioEventHandleObject>(jsonStr)!;

                        if (jsonObject != null && jsonObject.DeviceId == deviceID)
                        {
                            writelog($"jsonObject values: {item}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");

                            ICommodity _comdityAirAudioTmp = await _commSdk.GetCommodityAsync<IAirAudioCommodity>(new ItemId(item), CancellationToken.None);

                            if (RegisterEventsForAirAudio(_comdityAirAudioTmp))
                            {
                                jsonObject.airaudioIndex = item;
                                jsonObject.airaudioCommodity = _comdityAirAudioTmp;
                                airaudioList.Add(jsonObject);

                                writelog($"AirAudio {deviceID} Commodity events registered successfully");

                                return true;
                            }
                            else
                            {
                                writelog($"AirAudio {deviceID} Commodity events registered fail");

                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                writelog($"AirAudio{deviceID} RegisterEventsForAirAudioAsync Exception {e.Message}");

                return false;
            }

            return false;
        }

        private bool RegisterEventsForAirAudio(ICommodity _comdityAirAudio)
        {
            if (null == _comdityAirAudio)
            {
                writelog($"_comdityAirAudio == null");

                return false;
            }

            try
            {
                if (_comdityAirAudio is Dell.TechHub.Commodity.Peripheral.IAirAudioCommodity _AirAudiocom)
                {
                    _AirAudiocom.FirmwareVersionChanged += AirAudio_FirmwareVersionChanged;
                    //_AirAudiocom.BatteryLevelChanged += AirAudio_BatteryLevelChanged;
                    //_AirAudiocom.BatteryStatusChanged += AirAudio_BatteryStatusChanged;
                    _AirAudiocom.PairedHostNameChanged += AirAudio_PairedHostNameChanged;
                    _AirAudiocom.InstanceNumberChanged += AirAudio_InstanceNumberChanged;
                    _AirAudiocom.IsReadyChanged += AirAudio_IsReadyChanged;
                    _AirAudiocom.IsDirtyChanged += AirAudio_IsDirtyChanged;
                    _AirAudiocom.MicNoiseCancellationChanged += AirAudio_MicNoiseCancellationChanged;
                    _AirAudiocom.MicNCIncomingChanged += AirAudio_MicNCIncomingChanged;
                    _AirAudiocom.SidetoneChanged += AirAudio_SidetoneChanged;
                    _AirAudiocom.BusyLightChanged += AirAudio_BusyLightChanged;
                    _AirAudiocom.VoiceGuidanceChanged += AirAudio_VoiceGuidanceChanged;
                    _AirAudiocom.SelectedPresetChanged += AirAudio_SelectedPresetChanged;
                    _AirAudiocom.SidetoneLevelChanged += AirAudio_SidetoneLevelChanged;
                    _AirAudiocom.MuteStatusChanged += AirAudio_MuteStatusChanged;
                    _AirAudiocom.BandsGainChanged += AirAudio_BandsGainChanged;
                    _AirAudiocom.AncModeChanged += AirAudio_AncModeChanged;
                    _AirAudiocom.AncGainChanged += AirAudio_AncGainChanged;
                    _AirAudiocom.BoomMicChanged += AirAudio_BoomMicChanged;
                    _AirAudiocom.IsBoomMicSupportedChanged += AirAudio_BoomMicSupportedChanged;
                    _AirAudiocom.SerialNumberChanged += AirAudio_SerialNumberChanged;
                    //_AirAudiocom.WearDetectionChanged += AirAudio_WearDetectionChanged;
                    //_AirAudiocom.IsWearDetectionPauseMusicEnabledChanged += AirAudio_IsWearDetectionPauseMusicEnabledChanged;
                    //_AirAudiocom.IsWearDetectionMuteMicEnabledChanged += AirAudio_IsWearDetectionMuteMicEnabledChanged;
                    //_AirAudiocom.WearDetectionSensitivityChanged += AirAudio_WearDetectionSensitivityChanged;
                    //_AirAudiocom.WearDetectionQuickPauseChanged += AirAudio_WearDetectionQuickPauseChanged;

                    writelog($"AirAudiocom Commodity {_AirAudiocom.DeviceName}/{_AirAudiocom.DeviceId}/{_AirAudiocom.ModelNumber} events registered successfully");

                    return true;
                }
                else
                {
                    writelog($"_comdityAirAudio is not Dell.TechHub.Commodity.Peripheral.IAirAudioCommodity");

                    return false;
                }
            }
            catch (Exception e)
            {
                writelog($"Catch exception {e.Message} when run RegisterEventsForHeadset");

                return false;
            }
        }

        private void AirAudio_Connected(object sender, ConnectedArgs e)
        {
            Task<bool> result = RegisterEventsForAirAudioAsync(e.DeviceId);

            SendDTPEventToUI(CreateEventMsg("AirAudio", "AirAudio_Connected", e.DeviceId));

            writelog($"Catch event _AirAudio_Connected, register events result is {result.Result} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        //private void Headset_Disconnected(object sender, DisconnectedArgs e)
        //{
        //    _ = UnregisterEventsForAllHeadsetAsync();

        //    //_ = RegisterEventsForAllHeadsetAsync();

        //    //SendHeadsetEventToUI(CreateEventMsg("Headset", "Headset_Disconnected", e.DeviceId));

        //    writelog($"[Headset] Catch event _Headset_Disconnected, current devCount is {GetHeadsetDevsCountAsync().Result} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        //}

        //private void Headset_Connected(object sender, ConnectedArgs e)
        //{
        //    Task<int> headsets = GetHeadsetDevsCountAsync();
        //    bool result = RegisterEventsForHeadsetAsync(headsets.Result - 1).Result;

        //    //SendHeadsetEventToUI(CreateEventMsg("Headset", "Headset_Connected", e.DeviceId));

        //    writelog($"[Headset] Catch event _Headset_Connected, register evnet result is {result} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        //}

        private void AirAudio_MuteStatusChanged(object sender, MuteStatusChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_MuteStatusChanged",
                                    e.DeviceId, $"AirAudio_MuteStatusChanged:{e.MuteStatus.ToString()}"));

            writelog($"[Headset] Catch event AirAudio_MuteStatusChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_IsReadyChanged(object sender, IsReadyChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_IsReadyChanged",
                                    e.DeviceId, $"AirAudio_IsReadyChanged:{e.IsReady.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_IsReadyChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_IsDirtyChanged(object sender, IsDirtyChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_IsDirtyChanged",
                                    e.DeviceId, $"AirAudio_IsDirtyChanged:{e.IsDirty.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_IsDirtyChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_FirmwareVersionChanged(object sender, FirmwareVersionChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_FirmwareVersionChanged",
                                    e.DeviceId, $"AirAudio_FirmwareVersionChanged:{e.FirmwareVersion}"));

            writelog($"[AirAudio] Catch event AirAudio_FirmwareVersionChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }
        private void AirAudio_BatteryLevelChanged(object sender, BatteryLevelChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_BatteryLevelChanged",
                                    e.DeviceId, $"AirAudio_BatteryLevelChanged:{e.BatteryLevel.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_BatteryLevelChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_BatteryStatusChanged(object sender, BatteryStatusChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_BatteryStatusChanged",
                                    e.DeviceId, $"Headset_BatteryStatusChanged:{e.BatteryStatus.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_BatteryStatusChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_PairedHostNameChanged(object sender, PairedHostNameChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_PairedHostNameChanged", e.DeviceId,
                                      "AirAudio_PairedHostNameIndex:" + e.Index.ToString() + ";" +
                                      "AirAudio_PairedHostNameNewhostName:" + e.NewhostName.ToString()));

            writelog($"[AirAudio] Catch event AirAudio_PairedHostNameChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_InstanceNumberChanged(object sender, InstanceNumberChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_InstanceNumberChanged",
                                    e.DeviceId, $"AirAudio_InstanceNumberChanged:{e.InstanceNumber.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_InstanceNumberChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_MicNoiseCancellationChanged(object sender, MicNoiseCancellationChangedArgs e)
        {
            SendAirAudioEventToUI(CreateHeadsetEventMsg("AirAudio", "AirAudio_MicNoiseCancellationChanged",
                                    e.DeviceId, $"AirAudio_MicNoiseCancellationChanged:{e.MicNoiseCancellation.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_MicNoiseCancellationChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_MicNCIncomingChanged(object sender, MicNCIncomingChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_MicNCIncomingChanged",
                                    e.DeviceId, $"AirAudio_MicNCIncomingChanged:{e.MicNCIncoming.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_MicNCIncomingChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_SidetoneChanged(object sender, SidetoneChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_SidetoneChanged",
                                    e.DeviceId, $"AirAudio_SidetoneChanged:{e.Sidetone.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_SidetoneChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_BusyLightChanged(object sender, BusyLightChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_BusyLightChanged",
                                    e.DeviceId, $"AirAudio_BusyLightChanged:{e.BusyLight.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_BusyLightChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_VoiceGuidanceChanged(object sender, VoiceGuidanceChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_VoiceGuidanceChanged",
                                    e.DeviceId, $"AirAudio_VoiceGuidanceChanged:{e.VoiceGuidance.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_VoiceGuidanceChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_SelectedPresetChanged(object sender, SelectedPresetChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_SelectedPresetChanged",
                                    e.DeviceId, $"AirAudio_SelectedPresetChanged:{e.SelectedPreset.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_SelectedPresetChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_SidetoneLevelChanged(object sender, SidetoneLevelChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_SidetoneLevelChanged",
                                    e.DeviceId, $"AirAudio_SidetoneLevelChanged:{e.SidetoneLevel.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_SidetoneLevelChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_AncModeChanged(object sender, AncModeChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_AncModeChanged",
                                    e.DeviceId, $"AirAudio_AncModeChanged:{e.AncMode.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_AncModeChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_AncGainChanged(object sender, AncGainChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_AncGainChanged",
                                    e.DeviceId, $"AirAudio_AncGainChanged:{e.AncGain.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_AncGainChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_BoomMicSupportedChanged(object sender, IsBoomMicSupportedChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_BoomMicSupportedChanged",
                                    e.DeviceId, $"AirAudio_BoomMicSupportedChanged:{e.IsBoomMicSupported.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_BoomMicSupportedChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_SerialNumberChanged(object sender, SerialNumberChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_BoomMicSupportedChangedArgs",
                                    e.DeviceId, $"AirAudio_BoomMicSupportedChangedArgs:{e.SerialNumber}"));

            writelog($"[AirAudio] Catch event AirAudio_BoomMicSupportedChangedArgs : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_BandsGainChanged(object sender, BandsGainChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_BandsGainChanged", e.DeviceId,
                                      "AirAudio_Band1Gain:" + e.Band1Gain.ToString() + ";" +
                                      "AirAudio_Band2Gain:" + e.Band2Gain.ToString() + ";" +
                                      "AirAudio_Band3Gain:" + e.Band3Gain.ToString() + ";" +
                                      "AirAudio_Band4Gain:" + e.Band4Gain.ToString() + ";" +
                                      "AirAudio_Band5Gain:" + e.Band5Gain.ToString()));
            writelog($"[AirAudio] Catch event AirAudio_BandsGainChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_WearDetectionChanged(object sender, WearDetectionChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_WearDetectionChanged", e.DeviceId,
                                                 $"AirAudio_WearDetectionChanged:{e.IsGlobalEnabled.ToString() + ";" +
                            "AirAudio_IsWearDetectionPauseMusicEnabledChanged:" + e.IsPauseMusicEnabled.ToString() + ";" +
                               "AirAudio_IsWearDetectionMuteMicEnabledChanged:" + e.IsMuteMicEnabled.ToString() + ";" +
                                    "AirAudio_WearDetectionSensitivityChanged:" + e.Sensitivity.ToString() + ";" +
                                     "AirAudio_WearDetectionQuickPauseChanged:" + e.QuickPause.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_WearDetectionChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_WearDetectionSensitivityChanged(object sender, WearDetectionSensitivityChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_WearDetectionSensitivityChanged",
                                    e.DeviceId, $"AirAudio_WearDetectionSensitivityChanged:{e.WearDetectionSensitivity.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_WearDetectionSensitivityChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_IsWearDetectionPauseMusicEnabledChanged(object sender, IsWearDetectionPauseMusicEnabledChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_IsWearDetectionPauseMusicEnabledChanged",
                                    e.DeviceId, $"AirAudio_IsWearDetectionPauseMusicEnabledChanged:{e.IsPauseMusicEnabled.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_IsWearDetectionPauseMusicEnabledChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_IsWearDetectionMuteMicEnabledChanged(object sender, IsWearDetectionMuteMicEnabledChangedArgs e)
        {
            SendAirAudioEventToUI(CreateHeadsetEventMsg("AirAudio", "AirAudio_IsWearDetectionMuteMicEnabledChanged",
                                    e.DeviceId, $"AirAudio_IsWearDetectionMuteMicEnabledChanged:{e.IsWearDetectionMuteMicEnabled.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_IsWearDetectionMuteMicEnabledChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void AirAudio_WearDetectionQuickPauseChanged(object sender, WearDetectionQuickPauseChangedArgs e)
        {
            SendAirAudioEventToUI(CreateHeadsetEventMsg("AirAudio", "AirAudio_WearDetectionQuickPauseChanged",
                                    e.DeviceId, $"AirAudio_WearDetectionQuickPauseChanged:{e.WearDetectionQuickPause.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_WearDetectionQuickPauseChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }
        private void AirAudio_BoomMicChanged(object sender, BoomMicChangedArgs e)
        {
            SendAirAudioEventToUI(CreateAirAudioEventMsg("AirAudio", "AirAudio_BoomMicChanged",
                                    e.DeviceId, $"AirAudio_BoomMicChanged:{e.BoomMic.ToString()}"));

            writelog($"[AirAudio] Catch event AirAudio_BoomMicChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private string CreateAirAudioEventMsg(string devType, string eventType, string devID, string eventContent = "NewValue:NoContent")
        {
            writelog($"[AirAudio] Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}");
            return $"AirAudioEvent_5;Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}";
        }

        public void SendAirAudioEventToUI(string sendMsg)
        {
            UpdateUINotify airaudioEventNotify = new UpdateUINotify();
            airaudioEventNotify.UI_Field_Name = $"{sendMsg}";
            OnUIUpdateNotify(airaudioEventNotify);
        }
        private async Task<int> GetAirAudioDevsCountAsync()
        {
            try
            {
                var airaudios = await GetAirAudioDeviceItemsExAsync();
                if (!string.IsNullOrEmpty(airaudios))
                {

                    string[] parsedArray = airaudios.Split(new[] { ", " }, StringSplitOptions.None);

                    writelog("RegisterEventsForAllAirAudioAsync ********** " + airaudios.ToString() + " ********** ");
                    return parsedArray.Length;
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioDevsCountAsync - Exception: {ex.Message}");
                return 0;
            }

        }

        public async Task<string> GetAirAudioDeviceItemsExAsync()
        {
            try
            {
                _itemID = new ItemId(AirAudioItemID);

                if (_airaudioMethodInfo != null)
                {
                    var commodity = await GetCommodityInterfaceInstanceAsync(_airaudioMethodInfo);
                    if (commodity is ICommodity)
                    {
                        var value = GetPropertyValue(_airaudioInterfaceType, commodity, "DeviceItems");
                        writelog($"[DTPProxyPlugin] [AirAudio] GetDeviceItemsExAsync succeeded");

                        if (value != null)
                        {
                            return string.Join(", ", value);

                        }
                        else
                            return string.Empty;
                    }
                }

                writelog($"[DTPProxyPlugin] [AirAudio] GetDeviceItemsExAsync failed: Could not retrieve commodity interface");
                return string.Empty;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [AirAudio] GetDeviceItemsExAsync failed - Exception: {ex.Message}");
                return string.Empty;
            }
        }
        #endregion AirAudio Event

#if Support_210

        #region RtkHub Event
        /// <summary>
        /// Already connected do this
        /// </summary>
        /// <returns></returns>
        private async Task<bool> RegisterEventsForAllRtkHubAsync()
        {
            bool result = false;
            var rtkhubs = await GetRtkHubDevsCountAsync();
            if (rtkhubs > 0)
            {
                writelog($"RtkHub instance count: {rtkhubs} to register");

                for (int i = 0; i < rtkhubs; i++)
                {
                    result = await RegisterEventsForRtkHubAsync(i);

                    if (!result)
                    {
                        writelog($"[RtkHub] Register Events For RtkHub{i} fail, try un-register and register again");

                        result = await UnregisterEventsForRtkHubAsync(i);
                        result = await RegisterEventsForRtkHubAsync(i);
                        writelog($"[RtkHub] Retry register result is {result}");
                    }
                }
            }
            else
            {
                writelog($"[RtkHub] No any rtkhub instance to register.");
                return false;
            }
            return false;
        }

        /// <summary>
        /// Already connected do this
        /// </summary>
        /// <returns></returns>
        private async Task UnregisterEventsForAllRtkHubAsync()
        {
            bool result = false;
            var rtkhubs = await GetRtkHubDevsCountAsync();
            if (rtkhubs > 0)
            {
                writelog($"[RtkHub] instance count: {rtkhubs} to unregister.");

                for (int i = rtkhubs - 1; i >= 0; i--)
                {
                    result = await UnregisterEventsForRtkHubAsync(i);
                }
            }
            else
                writelog($"[RtkHub] No any rtkhub instance to unregister.");
        }

        private async Task<bool> RegisterEventsForRtkHubAsync(int index)
        {
            if (null == _commSdk || null == _comdity || index < 0)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<IRtkHubCommodity>(new ItemId($"DellPeripheral.RtkHub.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.IRtkHubCommodity _RtkHubcom)
                {
                    _RtkHubcom.FirmwareVersionChanged += RtkHub_FirmwareVersionChanged;
                    writelog($"RtkHub{index} Commodity events registered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"[RtkHub] {index} RegisterEventsForRtkHub Exception {e.Message}");

                return false;
            }

            return false;
        }

        private async Task<bool> UnregisterEventsForRtkHubAsync(int index)
        {
            if (null == _commSdk || null == _comdity || index < 0)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<IRtkHubCommodity>(new ItemId($"DellPeripheral.RtkHub.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.IRtkHubCommodity _RtkHubcom)
                {
                    _RtkHubcom.FirmwareVersionChanged -= RtkHub_FirmwareVersionChanged;
                    writelog($"[RtkHub] RtkHub{index} Commodity events unregistered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"[RtkHub] RtkHub{index} UnregisterEventsForRtkHub Exception {e.Message}");
                return false;
            }
            return false;
        }

        private bool UnregisterEventsForRtkHub(RtkHubEventHandleObject obj)
        {
            writelog($"[RtkHub] UnregisterEventsForRtkHub in ... ");
            if (obj.rtkhubCommodity is Dell.TechHub.Commodity.Peripheral.IRtkHubCommodity _RtkHubcom)
            {
                _RtkHubcom.FirmwareVersionChanged -= RtkHub_FirmwareVersionChanged;
                writelog($"RtkHub {obj.rtkhubIndex}/{obj.ModelNumber} Commodity events unregistered successfully");

                return true;
            }
            else
                writelog($"obj.rtkhubCommodity is not Dell.TechHub.Commodity.Peripheral.IRtkHubCommodity for {obj.ModelNumber}");

            return false;
        }

        private async Task<bool> UnregisterEventsForRtkHubAsync(string devcieID)
        {
            writelog($"[RtkHub] UnregisterEventsForRtkHubAsync in ... ");
            if (devcieID == null || devcieID == string.Empty || rtkhubList.Count == 0)
            {
                writelog($"devcieID == string.Empty || devcieID == null || rtkhubList.Count == 0");

                return false;
            }

            try
            {
                // find _comdity object for this device
                writelog($"Search {devcieID} from rtkhubList for Unregister Events");

                bool result = false;
                foreach (var item in rtkhubList)
                {
                    if (item.DeviceId == devcieID)
                    {
                        result = true;
                        writelog($"Found object {item.DeviceName} from rtkhubList for Unregister Events");
                        result = UnregisterEventsForRtkHub(item);
                        writelog($"UnregisterEventsForRtkHub result is {result}");
                        result = rtkhubList.Remove(item);
                        writelog($"rtkhubList.Remove(item) result is {result}");
                        break;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                writelog($"RtkHub{devcieID} UnregisterEventsForRtkHubAsync Exception {e.Message}");

                return false;
            }
        }

        private void RtkHub_Disconnected(object sender, DisconnectedArgs e)
        {
            writelog($"[RtkHub] RtkHub_Disconnected in ... ");

            Task<bool> result = UnregisterEventsForRtkHubAsync(e.DeviceId);

            SendDTPEventToUI(CreateRtkHubEventMsg("RtkHub", "RtkHub_Disconnected", e.DeviceId));

            writelog($"[RtkHub] Catch event RtkHub_Disconnected, unregister events result is {result.Result}, current devCount is {rtkhubList.Count} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private async Task<bool> RegisterEventsForRtkHubAsync(string deviceID)
        {
            writelog($"[RtkHub] RegisterEventsForRtkHubAsync in ... ");
            if (null == _comdityRtkHub || deviceID == null || deviceID == string.Empty)
            {
                writelog($"null == _comdityRtkHub || deviceID == null || deviceID == string.Empty");

                return false;
            }

            try
            {
                if (_comdityHeadset is Dell.TechHub.Commodity.Peripheral.IRtkHubCommodity _RtkHubComObj)
                {
                    writelog($"connected RtkHubComObj.DeviceItems = {_RtkHubComObj.DeviceItems.Length}");

                    int i = 0;
                    foreach (var item in _RtkHubComObj.DeviceItems)
                    {
                        writelog($"connected RtkHubComObj.DeviceItems[{i}] = {item}");
                        string jsonStr = _RtkHubComObj.DeviceItemsEx[i++].ToString();
                        writelog($"connected RtkHubComObj.DeviceItems = {jsonStr}");

                        RtkHubEventHandleObject jsonObject = JsonSerializer.Deserialize<RtkHubEventHandleObject>(jsonStr)!;

                        if (jsonObject != null && jsonObject.DeviceId == deviceID)
                        {
                            writelog($"jsonObject values: {item}, {jsonObject.DeviceName}, {jsonObject.DeviceId}, {jsonObject.ModelNumber}");

                            ICommodity _comdityRtkHubTmp = await _commSdk.GetCommodityAsync<IRtkHubCommodity>(new ItemId(item), CancellationToken.None);

                            if (RegisterEventsForRtkHub(_comdityRtkHubTmp))
                            {
                                jsonObject.rtkhubIndex = item;
                                jsonObject.rtkhubCommodity = _comdityRtkHubTmp;
                                rtkhubList.Add(jsonObject);

                                writelog($"[RtkHub] {deviceID} Commodity events registered successfully");

                                return true;
                            }
                            else
                            {
                                writelog($"[RtkHub] {deviceID} Commodity events registered fail");

                                return false;
                            }
                        }
                    }
                }
                else
                {
                    writelog($"_comdityRtkHub is not Dell.TechHub.Commodity.Peripheral.IRtkHubCommodity");
                    return false;
                }
            }
            catch (Exception e)
            {
                writelog($"RtkHub{deviceID} RegisterEventsForRtkHubAsync Exception {e.Message}");

                return false;
            }
            writelog($"[RtkHub] RegisterEventsForRtkHubAsync return false ... ");
            return false;
        }

        private bool RegisterEventsForRtkHub(ICommodity _comdityRtkHub)
        {
            writelog($"[RtkHub] RegisterEventsForRtkHub in ... ");
            if (null == _comdityRtkHub)
            {
                writelog($"_comdityRtkHub == null");

                return false;
            }

            try
            {
                if (_comdityHeadset is Dell.TechHub.Commodity.Peripheral.IRtkHubCommodity _RtkHubcom)
                {
                    _RtkHubcom.FirmwareVersionChanged += RtkHub_FirmwareVersionChanged;
                    writelog($"RtkHubcom Commodity {_RtkHubcom.DeviceName}/{_RtkHubcom.DeviceId}/{_RtkHubcom.ModelNumber} events registered successfully");

                    return true;
                }
                else
                {
                    writelog($"_comdityRtkHub is not Dell.TechHub.Commodity.Peripheral.IRtkHubCommodity");

                    return false;
                }
            }
            catch (Exception e)
            {
                writelog($"Catch exception {e.Message} when run RegisterEventsForRtkHub");
                return false;
            }
        }

        private void RtkHub_Connected(object sender, ConnectedArgs e)
        {
            writelog($"[RtkHub] RtkHub_Connected in ... ");

            Task<bool> result = RegisterEventsForRtkHubAsync(e.DeviceId);

            SendDTPEventToUI(CreateRtkHubEventMsg("RtkHub", "RtkHub_Connected", e.DeviceId));

            writelog($"[RtkHub] Catch event RtkHub_Connected, register events result is {result.Result} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void RtkHub_FirmwareVersionChanged(object sender, FirmwareVersionChangedArgs e)
        {
            SendHeadsetEventToUI(CreateRtkHubEventMsg("RtkHub", "RtkHub_FirmwareVersionChanged",
                                    e.DeviceId, $"RtkHub_FirmwareVersionChanged:{e.FirmwareVersion}"));

            writelog($"[RtkHub] Catch event RtkHub_FirmwareVersionChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private string CreateRtkHubEventMsg(string devType, string eventType, string devID, string eventContent = "NewValue:NoContent")
        {
            writelog($"[RtkHub] Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}");
            return $"RtkHubEvent_5;Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}";
        }

        #endregion RtkHub Event

        #region RtkHub Get

        private async Task<int> GetRtkHubDevsCountAsync()
        {
            var rtkhubs = await GetRtkHubDeviceItemsExAsync();
            if (rtkhubs != null)
            {
                Trace.WriteLine("GetRtkHubDevsCountAsync ********** " + rtkhubs.ToString() + " ********** ");
                return rtkhubs.Count;
            }
            else
                return 0;
        }

        public async Task<JArray> GetRtkHubDeviceItemsExAsync()
        {
            try
            {
                _itemID = new ItemId(RtkHubItemID);

                if (_rtkhubMethodInfo != null)
                {
                    var commodity = await GetCommodityInterfaceInstanceAsync(_rtkhubMethodInfo);
                    if (commodity is ICommodity)
                    {
                        var value = GetPropertyValue(_rtkhubInterfaceType, commodity, "DeviceItemsEx");
                        writelog($"[DTPProxyPlugin] [RtkHub] GetDeviceItemsExAsync succeeded");
                        return value == null ? new JArray() : (JArray)value;
                    }
                }

                writelog($"[DTPProxyPlugin] [RtkHub] GetDeviceItemsExAsync failed: Could not retrieve commodity interface");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [RtkHub] GetDeviceItemsExAsync failed - Exception: {ex.Message}");
                return null;
            }
        }


        #endregion RtkHub Get
#endif

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