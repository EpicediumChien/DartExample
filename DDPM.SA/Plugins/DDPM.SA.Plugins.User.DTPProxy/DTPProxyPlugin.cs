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

namespace DDPM.SA.Plugins.User.DTPProxy
{
    [Plugin(DDPM.SA.Common.IDs.DDPM_DTP_Proxy_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [PluginRequires(Id = "{743D1C20-7A3E-4562-8D3E-C58F6ADFC050}", Version = "1.0.0", AllowDynamicResolving = true)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IDTPProxyPlugin) })]
    public class DTPProxyPlugin : BaseAgentPlugin, IDTPProxyPlugin
    {
        private object _PeripheralLock = new object();

        #region Properties and fields

        private const string pluginName = "DTPProxyPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements DTP Proxy Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements DTP Proxy Plugin.";

        private readonly IAgent _agent;
        private ICommodityClientSdk _commSdk;

        //private ClientAppId appId = new ClientAppId("{675f1370-b7ce-4113-8d6e-a128ee3bb74b}");
        private ClientAppId appId = new ClientAppId("{b397b9b3-04cb-4cdf-8a79-852d63cf4801}");

        private readonly object _PluginConditionLock = new object();

        private Type _mouseInterfaceType;
        private Type _keyboardInterfaceType;
        private Type _penInterfaceType;
        private Type _speakerInterfaceType;
        private Type _dockInterfaceType;
        private Type _headsetInterfaceType;
        private Type _webcamInterfaceType;
        private MethodInfo _mouseMethodInfo;
        private MethodInfo _keyboardMethodInfo;
        private MethodInfo _penMethodInfo;
        private MethodInfo _speakerMethodInfo;
        private MethodInfo _dockMethodInfo;
        private MethodInfo _headsetMethodInfo;
        private MethodInfo _webcamMethodInfo;

        private ItemId _itemID;
        private ICommodity _comdity;
        private const string PenItemID = "DellPeripheral.Pen";
        private const string PenItemID0 = "DellPeripheral.Pen.0";

        public const string PluginLogId = "DTPProxy";

        #endregion

        #region Constructor

        public DTPProxyPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;

            writelog("DTPProxyPlugin constructor ...");
            writelog($"Initializing the Commodity Client SDK...");
            var commodity = new ConnectedArgs("");

            InitializeDTPProxy();
            writelog($"Initialized successfully");
        }

        #endregion

        public event EventHandler<DeviceChangedEventArgs> Notify;

        public event EventHandler<bool> UpdateNotify;

        public void NotifyNow()
        {
            OnNotify(new DeviceChangedEventArgs());
        }

        public async Task<int> GetDpiValue(string itemID)
        {
            _itemID = new ItemId(itemID);

            if (_mouseMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_mouseInterfaceType, commodity, "DpiValue");
                    return (int)value;
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

        public async Task SetDPIValue(string itemID, int newValue)
        {
            _itemID = new ItemId(itemID);

            if (_mouseMethodInfo != null)
            {
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
            else
            {
                Debug.WriteLine($"[SetDPIValue]Could not retrieve the Commodity Interface for the  {_itemID}  item. _mouseMethodInfo is null");
                writelog($"[SetDPIValue]Could not retrieve the Commodity Interface for the  {_itemID}  item. _mouseMethodInfo is null");
            }

        }

        #region Webcam
        public async Task<JArray> GetPresetProfiles(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return (JArray)""; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "PresetProfiles");
                Debug.WriteLine($"{value}");
                return (JArray)value;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return (JArray)"";
            }
        }
        public async Task<JArray> GetCustomProfiles(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return (JArray)""; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "CustomProfiles");
                Debug.WriteLine($"{value}");
                return (JArray)value;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return (JArray)"";
            }
        }

        public async Task<string> GetProfileName(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return ""; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "ProfileName");
                return (string)value;
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
                return (string)value;
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
                return (int)value;
            }
            else
            {
                Debug.WriteLine($"[GetBrightnessValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"[GetBrightnessValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return -1;
            }
        }

        public async Task<string> GetCameraFirmwareVersion(string itemID)
        {
            _itemID = new ItemId(itemID);

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "FirmwareVersion");
                    return (string)value;
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

        public async Task<bool> CheckIsPropertyFOVSupported(string itemID)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPropertyFOVSupported");
                    return (bool)value;
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

        public async Task<int> GetFieldOfViewValue(string itemID)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return -1; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "FieldOfView");
                    return (int)value;
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

        public async Task<bool> CheckIsPropertyHDRSupported(string itemID)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPropertyHDRSupported");
                    return (bool)value;
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

        public async Task<bool> GetIsHDROnValue(string itemID)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsHDROn");
                    return (bool)value;
                }
                else
                {
                    Debug.WriteLine($"[GetIsHDROnValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetIsHDROnValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
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

        public async Task SetIsHDROnValue(string itemID, bool newValue)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_webcamInterfaceType, commodity, "IsHDROn", newValue);
                }
                else
                {
                    Debug.WriteLine($"[SetIsHDROnValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[SetIsHDROnValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetIsHDROnValue]Could not retrieve the Commodity Interface for the  {_itemID}  item. _webcamMethodInfo is null");
                writelog($"[SetIsHDROnValue]Could not retrieve the Commodity Interface for the  {_itemID}  item. _webcamMethodInfo is null");
            }

        }

        public async Task<bool> CheckIsPropertyAntiFlickerSupported(string itemID)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPropertyHDRSupported");
                    return (bool)value;
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

        public async Task<int> GetAntiFlickerValue(string itemID)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return -1; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "AntiFlicker");
                    return (int)value;
                }
                else
                {
                    Debug.WriteLine($"[GetAntiFlickerValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetAntiFlickerValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return -1;
                }
            }
            else
            {
                Debug.WriteLine($"[GetAntiFlickerValue]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetAntiFlickerValue]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return -1;
            }

        }

        public async Task SetAntiFlickerValue(string itemID, int newValue)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_webcamInterfaceType, commodity, "AntiFlicker", newValue);
                }
                else
                {
                    Debug.WriteLine($"[SetAntiFlickerValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[SetAntiFlickerValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetAntiFlickerValue]Could not retrieve the Commodity Interface for the  {_itemID}  item. _webcamMethodInfo is null");
                writelog($"[SetAntiFlickerValue]Could not retrieve the Commodity Interface for the  {_itemID}  item. _webcamMethodInfo is null");
            }

        }

        public async Task<bool> CheckIsPropertyAutoFramingSupported(string itemID)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPropertyAutoFramingSupported");
                    return (bool)value;
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

        public async Task<bool> GetIsAutoFramingOnValue(string itemID)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsAutoFramingOn");
                    return (bool)value;
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

        public async Task SetIsAutoFramingOnValue(string itemID, bool newValue)
        {
            //_itemID = new ItemId(itemID);
            if (!await GetItemIDAsync("Webcam", itemID))
            { return; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_webcamInterfaceType, commodity, "IsAutoFramingOn", newValue);
                }
                else
                {
                    Debug.WriteLine($"[SetIsAutoFramingOnValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[SetIsAutoFramingOnValue]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetIsAutoFramingOnValue]Could not retrieve the Commodity Interface for the  {_itemID}  item. _webcamMethodInfo is null");
                writelog($"[SetIsAutoFramingOnValue]Could not retrieve the Commodity Interface for the  {_itemID}  item. _webcamMethodInfo is null");
            }

        }

        public async Task SetProfile(string Guid, string newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

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
            { return; }

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
            { return; }

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
            { return; }

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
        public async Task SetZoom(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Zoom", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetAutoFramingSensitivity(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "AutoFramingSensitivity", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetAutoFramingFrameSize(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "AutoFramingSensitivity", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetIsAutoFramingOn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsAutoFramingOn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetIsAutoFramingTransitionOn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsAutoFramingTransitionOn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetFieldOfView(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "FieldOfView", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetIsFocusOn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsFocusOn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetFocus(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Focus", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetPriority(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Priority", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetIsHDROn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsHDROn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetIsAutoWhiteBalanceOn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsAutoWhiteBalanceOn", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetAutoWhiteBalance(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "AutoWhiteBalance", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetBrightness(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Brightness", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetSharpness(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Sharpness", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetContrast(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Contrast", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetSaturation(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "Saturation", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }
        public async Task SetAntiFlicker(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

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
            { return; }

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
            { return; }

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
            { return; }

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
            { return; }

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
            { return; }

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
            { return; }

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
            { return; }

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
            { return; }

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
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsWalkAwayLockEnable", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetIsPrioritizeExternalWebcam(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_webcamInterfaceType, commodity, "IsPrioritizeExternalWebcam", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task ResetToDefault_webcam(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return; }

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

        public async Task<int> GetWALTime(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "WALTime");
                    return (int)value;
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

            /*
            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "WALTime");
                return (int)value;
            }
            else
            {
                Debug.WriteLine($"[GetWALTime]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"[GetWALTime]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return -1;
            }
            */
        }

        public async Task<int> GetSnooze(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "Snooze");
                    return (int)value;
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

            /*
            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "Snooze");
                return (int)value;
            }
            else
            {
                Debug.WriteLine($"[GetSnooze]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"[GetSnooze]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return -1;
            }
            */
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
                    return (int)value;
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

            /*
            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "SnoozeLength");
                return (int)value;
            }
            else
            {
                Debug.WriteLine($"[GetSnoozeLength]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                writelog($"[GetSnoozeLength]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                return -1;
            }
            */
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
                    return (bool)value;
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
                    return (bool)value;
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
                    return (bool)value;
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

        public async Task<bool> GetIsPrioritizeExternalWebcam(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPrioritizeExternalWebcam");
                    return (bool)value;
                }
                else
                {
                    Debug.WriteLine($"[GetIsPrioritizeExternalWebcam]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetIsPrioritizeExternalWebcam]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsPrioritizeExternalWebcam]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetIsPrioritizeExternalWebcam]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
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
                "Pen" => _penMethodInfo,
                "Webcam" => _webcamMethodInfo,
                "Headset" => _headsetMethodInfo,
                "Speaker" => _speakerMethodInfo,
                _ => null
            };
            Type interfaceType = type switch
            {
                "Pen" => _penInterfaceType,
                "Webcam" => _webcamInterfaceType,
                "Headset" => _headsetInterfaceType,
                "Speaker" => _speakerInterfaceType,
                _ => null
            };


            if (methodInfo == null)
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface to get Guid");
                writelog($"Could not retrieve the Commodity Interface to get Guid");
                return false;
            }

            int i = 0;
            string item = "DellPeripheral";
            while (i < 10)
            {
                _itemID = new ItemId($"{item}.{type}.{i}");
                Debug.WriteLine($"{_itemID}");
                Trace.WriteLine($"{_itemID}");
                if (await GetCommodityInterfaceInstanceAsync(methodInfo) is ICommodity commodity)
                {
                    Debug.WriteLine($"{commodity.GetType}");
                    var value = GetPropertyValue(interfaceType, commodity, "DeviceId");
                    Debug.WriteLine((string)value);
                    if ((string)value == guid)
                    { return true; }
                }
                i++;
            }
            Debug.WriteLine($"Not found {type} GUID: {guid}");
            writelog($"Not found {type} GUID: {guid}");
            Trace.WriteLine($"Not found {type} GUID: {guid}");
            return false;
        }
        #endregion

        #region Pen

        public async Task<string> PairingPen()
        {
            _itemID = new ItemId(PenItemID);
            if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_penInterfaceType, commodity, "Pair");
                Debug.WriteLine($"Pen Pair value: {value}");
                return (string)value;
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
                    return (JArray)value;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    return (JArray)"";
                }
            }
            else
            {
                Debug.WriteLine($"[GetDeviceItemsEx]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetDeviceItemsEx]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                return (JArray)"";
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return (bool)value;
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
                    return (bool)value;
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
                    return (bool)value;
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
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetEraserDoublePressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetEraserDoublePressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
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
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetEraserLongPressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetEraserLongPressSettingCould not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        public async Task SetEraserSinglePressSetting(string itemID, byte[] newValue)
        {
            _itemID = new ItemId(itemID);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_penInterfaceType, commodity, "EraserSinglePressSetting", newValue);
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetEraserSinglePressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetEraserSinglePressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
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
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetIsSideBottomButtonHoverClick]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetIsSideBottomButtonHoverClick]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
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
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetIsSideTopButtonHoverClick]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetIsSideTopButtonHoverClick]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
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
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetMenuSinglePressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetMenuSinglePressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
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
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetMenuCenterRightClickSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetMenuCenterRightClickSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
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
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetSideBottomSwitchSinglePressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetSideBottomSwitchSinglePressSetting]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
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
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
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
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetTiltSensitivity]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetTiltSensitivity]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
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
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Debug.WriteLine($"[SetTipSensitivity]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
                writelog($"[SetTipSensitivity]Could not retrieve the Commodity Interface for the  {_itemID}  item. _penMethodInfo is null");
            }
        }

        #endregion

        #region WiredAudio

        public async Task SetFactoryResetAsyncValueForHeadset(string Guid, bool newValue)
        {
            Trace.WriteLine("SetFactoryResetAsyncValue **********" + Guid + " || " + newValue.ToString());
            if (!await GetItemIDAsync("Headset", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_headsetInterfaceType, commodity, "FactoryReset", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
            }
        }

        #endregion

        #region WiredAudio
        public async Task SetBassAsync(string Guid, int newValue)
        {
            Trace.WriteLine("SetBassAsync **********" + Guid + " || " + newValue.ToString());
            if (!await GetItemIDAsync("Speaker", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_speakerInterfaceType, commodity, "Bass", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task<int> GetBassAsync(string Guid)
        {
            if (!await GetItemIDAsync("Speaker", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_speakerInterfaceType, commodity, "Bass");
                Trace.WriteLine("GetBassAsync **********" + Guid + " || " + value.ToString());
                return (int)value;
            }
            else
            {
                Debug.WriteLine($"[GetBassAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"[GetBassAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                return -1;
            }
        }

        public async Task SetMidRangeAsync(string Guid, int newValue)
        {
            Trace.WriteLine("SetMidRangeAsync **********" + Guid + " || " + newValue.ToString());
            if (!await GetItemIDAsync("Speaker", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_speakerInterfaceType, commodity, "MidRange", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task<int> GetMidRangeAsync(string Guid)
        {
            if (!await GetItemIDAsync("Speaker", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_speakerInterfaceType, commodity, "MidRange");
                Trace.WriteLine("GetBassAsync **********" + Guid + " || " + value.ToString());
                return (int)value;
            }
            else
            {
                Debug.WriteLine($"[GetBasGetMidRangeAsyncsAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"[GetMidRangeAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                return -1;
            }
        }
        public async Task SetTrebleAsync(string Guid, int newValue)
        {
            Trace.WriteLine("SetTrebleAsync **********" + Guid + " || " + newValue.ToString());
            if (!await GetItemIDAsync("Speaker", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_speakerInterfaceType, commodity, "Treble", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task<int> GetTrebleAsync(string Guid)
        {
            if (!await GetItemIDAsync("Speaker", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_speakerInterfaceType, commodity, "Treble");
                Trace.WriteLine("GetTrebleAsync **********" + Guid + " || " + value.ToString());
                return (int)value;
            }
            else
            {
                Debug.WriteLine($"[GetTrebleAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"[GetTrebleAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                return -1;
            }
        }

        //--------------------------------------
        public async Task SetIsWiredAudioMicMuteSoundEnableAsync(string Guid, bool newValue)
        {
            Trace.WriteLine("SetIsWiredAudioMicMuteSoundEnableAsync **********" + Guid + " || " + newValue.ToString());
            if (!await GetItemIDAsync("Speaker", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_speakerInterfaceType, commodity, "IsWiredAudioMicMuteSoundEnable", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task<bool> GetIsWiredAudioMicMuteSoundEnableAsync(string Guid)
        {
            if (!await GetItemIDAsync("Speaker", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsWiredAudioMicMuteSoundEnable");
                Trace.WriteLine("GetIsWiredAudioMicMuteSoundEnableAsync **********" + Guid + " || " + value.ToString());
                return (bool)value;
            }
            else
            {
                Debug.WriteLine($"[GetIsWiredAudioMicMuteSoundEnableAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"[GetIsWiredAudioMicMuteSoundEnableAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                return false;
            }
        }

        //--------------------------------------
        public async Task SetWiredAudioVolumeAdjustmentToneAsync(string Guid, int newValue)
        {
            Trace.WriteLine("SetIsWiredAudioIMicNSEnableValue **********" + Guid + " || " + newValue.ToString());
            if (!await GetItemIDAsync("Speaker", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_speakerInterfaceType, commodity, "WiredAudioVolumeAdjustmentTone", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task<int> GetWiredAudioVolumeAdjustmentToneAsync(string Guid)
        {
            if (!await GetItemIDAsync("Speaker", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_speakerInterfaceType, commodity, "WiredAudioVolumeAdjustmentTone");
                Trace.WriteLine("GetWiredAudioVolumeAdjustmentToneAsync **********" + Guid + " || " + value.ToString());
                return (int)value;
            }
            else
            {
                Debug.WriteLine($"[GetWiredAudioVolumeAdjustmentToneAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"[GetWiredAudioVolumeAdjustmentToneAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                return -1;
            }
        }

        //--------------------------------------
        public async Task SetIsWiredAudioIMicNSEnableValue(string Guid, bool newValue)
        {
            Trace.WriteLine("SetIsWiredAudioIMicNSEnableValue **********" + Guid + " || " + newValue.ToString());
            if (!await GetItemIDAsync("Speaker", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_speakerInterfaceType, commodity, "IsWiredAudioIMicNSEnable", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task<bool> GetIsWiredAudioIMicNSEnableValueAsync(string Guid)
        {
            if (!await GetItemIDAsync("Speaker", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsWiredAudioIMicNSEnable");
                Trace.WriteLine("GetIsWiredAudioIMicNSEnableValueAsync **********" + Guid + " || " + value.ToString());
                return (bool)value;
            }
            else
            {
                Debug.WriteLine($"[GetIsWiredAudioIMicNSEnableValueAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"[GetIsWiredAudioIMicNSEnableValueAsync]Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                return false;
            }
        }
        //--------------------------------------
        public async Task SetResetToDefaultValueAsync(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Speaker", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_speakerInterfaceType, commodity, "ResetToDefault", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_speakerInterfaceType} for the {_itemID} item.");
            }
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();
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
        private void writelog(string text, log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = "[DTPProxyPlugin] " + text;
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
                    Debug.WriteLine($"\n{assembly.FullName}, {assembly.Location}");
                    if (type is not null)
                        return type;
                }
                catch (Exception ex)
                {
                    writelog($"Failed to get exported type from assembly {assembly.FullName}:{ex}");
                    Debug.WriteLine($"\n{ex}");
                }
            }
            return null;
        }

        private async Task<ICommodity> GetCommodityInterfaceInstanceAsync(MethodInfo methodInfo)
        {
            try
            {
                dynamic rawResult = methodInfo.Invoke(_commSdk, new object[] { _itemID, new CancellationTokenSource().Token });
                Debug.WriteLine($"rawResult: {rawResult}");
                return rawResult is null ? null : (ICommodity)await rawResult;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"\nError handling {_mouseInterfaceType}'s {_itemID} item.\n{ex}");
                writelog($"\nError handling {_mouseInterfaceType}'s {_itemID} item.\n{ex}");
                return null;
            }
        }

        private void InitializeDTPProxy()
        {
            if (_commSdk != null)
                return;

            _commSdk = (ICommodityClientSdk)_agent.PluginManager.FindPluginByType(typeof(ICommodityClientSdk));

            if (_commSdk != null)
            {
                _ = Task.Run(async () =>
                {
                    writelog($"Find IMouseCommodity Init time : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
                    await _commSdk.InitializeAsync(appId, new CancellationTokenSource().Token);
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

                    _ = RegisterEventAsync();
                });
            }
            else
            {
                if (_commSdk is IFrameworkPluginConditionNotification pluginCondition)
                {
                    pluginCondition.PluginConditionChangeHandler += OnDTPProxyPluginConditionChangeHandler;
                    GetCurrentDTPProxyPluginCondition();
                }
            }
        }

        private async Task RegisterEventAsync()
        {
            writelog($"Register Commodity event...");
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

            writelog($"Register Commodity event...");
            _comdity = await _commSdk.GetCommodityAsync<IHeadsetCommodity>(new ItemId("DellPeripheral.Headset"), CancellationToken.None);
            if (_comdity is Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity _headsetcom)
            {
                try
                {
                    _headsetcom.IsReadyChanged += _headsetcomdity_IsReadyChanged;
                    _headsetcom.FirmwareVersionChanged += _headsetcomdity_FirmwareVersionChanged;
                    //_headsetcom.AncModeChanged += _comdity_AncModeChange;
                    //_headsetcom.Connected += _comdity_Connected;
                    //_headsetcom.Disconnected += _comdity_Disconnected;
                    writelog($"Headset Commodity event registered");
                }
                catch (Exception e)
                {
                    writelog($"Find IHeadsetCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
            }

            writelog($"Register Commodity event...");
            _comdity = await _commSdk.GetCommodityAsync<ISpeakerCommodity>(new ItemId("DellPeripheral.Speaker"), CancellationToken.None);
            if (_comdity is Dell.TechHub.Commodity.Peripheral.ISpeakerCommodity _speakercom)
            {
                try
                {
                    _speakercom.IsIMicNSEnabledChanged += _speakercomdity_IsIMicNSEnabledChanged;
                    _speakercom.VolumeAdjustmentToneChanged += _speakercomdity_VolumeAdjustmentToneChanged;
                    _speakercom.IsMicMuteSoundEnabledChanged += _speakercomdity_IsMicMuteSoundEnabledChanged;
                    writelog($"Speaker Commodity event registered");
                }
                catch (Exception e)
                {
                    writelog($"Find ISpeakerCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
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
                        //writelog($"{nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in an error condition");
                        //_DisplayManagerPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        //_DisplayManagerPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        //_DisplayManagerPluginCondition = pluginCondition;
                    }
                }
            });
        }

        private object GetPropertyValue(Type interfaceType, ICommodity commodity, string property)
        {
            try
            {
                Debug.WriteLine($"{commodity.GetType().Name}");
                return interfaceType.GetProperty(property).GetGetMethod().Invoke(commodity, null);
            }
            catch (Exception ex)
            {
                writelog($"Error while Getting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
                Debug.WriteLine($"Error while Getting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
                return null;
            }
        }

        private void SetPropertyValue(Type interfaceType, ICommodity commodity, string property, object value)
        {
            Debug.WriteLine($"ItemID: {_itemID}; Type: {interfaceType.Name}; property: {property}; value: {value}");
            try
            {
                interfaceType.GetProperty(property).GetSetMethod().Invoke(commodity, new[] { value });
            }
            catch (Exception ex)
            {
                writelog($"Error while setting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
            }
        }

        private void SetPropertyValue(Type interfaceType, ICommodity commodity, string property, byte[] value)
        {
            Debug.WriteLine($"ItemID: {_itemID}; Type: {interfaceType.Name}; property: {property}; value: {Encoding.UTF8.GetString(value)}");
            try
            {
                interfaceType.GetProperty(property).GetSetMethod().Invoke(commodity, new[] { value });
            }
            catch (Exception ex)
            {
                writelog($"Error while setting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
            }
        }
        private void _comdity_Disconnected(object sender, DisconnectedArgs e)
        {
            Debug.WriteLine($"Disconnected Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
        }

        private void _comdity_Connected(object sender, ConnectedArgs e)
        {
            Debug.WriteLine($"Connected Device ID: {e.DeviceId} !!!!!!!!!!!!!!!");
        }

        #region Headset

        private void _headsetcomdity_IsReadyChanged(object sender, IsReadyChangedArgs e)
        {
            DeviceChangedEventArgs _EventArgs = new();
            _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
            //_EventArgs.device_peripherals = deviceInfo;
            _EventArgs.changedProperty = "IsReadyChanged";
            OnNotify(_EventArgs);
            Debug.WriteLine($"IsReadyChanged Device ID: {e.DeviceId} ");
        }
        private void _headsetcomdity_FirmwareVersionChanged(object sender, FirmwareVersionChangedArgs e)
        {
            DeviceChangedEventArgs _EventArgs = new();
            _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
            //_EventArgs.device_peripherals = deviceInfo;
            _EventArgs.changedProperty = "FirmwareVersionChanged";
            OnNotify(_EventArgs);
            Debug.WriteLine($"FirmwareVersionChanged Device ID: {e.DeviceId} ");
        }
        private void _headsetcomdity_AncModeChange(object sender, AncModeChangedArgs e)
        {
            DeviceChangedEventArgs _EventArgs = new();
            _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
            //_EventArgs.device_peripherals = deviceInfo;
            _EventArgs.changedProperty = "AncModeChange";
            OnNotify(_EventArgs);
            Debug.WriteLine($"AncModeChange Device ID: {e.DeviceId} ");
        }
        #endregion

        #region Speaker

        private void _speakercomdity_IsIMicNSEnabledChanged(object sender, IsIMicNSEnabledChangedArgs e)
        {
            DeviceChangedEventArgs _EventArgs = new();
            _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
            //_EventArgs.device_peripherals = deviceInfo;
            _EventArgs.changedProperty = "IsIMicNSEnabledChanged";
            OnNotify(_EventArgs);
            Debug.WriteLine($"IsIMicNSEnabledChanged Device ID: {e.DeviceId} ");
        }

        private void _speakercomdity_VolumeAdjustmentToneChanged(object sender, VolumeAdjustmentToneChangedArgs e)
        {
            DeviceChangedEventArgs _EventArgs = new();
            _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
            //_EventArgs.device_peripherals = deviceInfo;
            _EventArgs.changedProperty = "VolumeAdjustmentToneChanged(";
            Debug.WriteLine($"VolumeAdjustmentToneChanged Device ID: {e.DeviceId} ");
        }

        private void _speakercomdity_IsMicMuteSoundEnabledChanged(object sender, IsMicMuteSoundEnabledChangedArgs e)
        {
            DeviceChangedEventArgs _EventArgs = new();
            _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
            //_EventArgs.device_peripherals = deviceInfo;
            _EventArgs.changedProperty = "IsMicMuteSoundEnabledChanged(";
            Debug.WriteLine($"IsMicMuteSoundEnabledChanged Device ID: {e.DeviceId} ");
        }
        #endregion
    }
}