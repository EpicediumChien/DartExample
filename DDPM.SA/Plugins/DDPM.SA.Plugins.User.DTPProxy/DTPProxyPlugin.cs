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
using DPeMPublic.Common.Enums;

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
        private Type _dongleInterfaceType;
        private MethodInfo _mouseMethodInfo;
        private MethodInfo _keyboardMethodInfo;
        private MethodInfo _penMethodInfo;
        private MethodInfo _speakerMethodInfo;
        private MethodInfo _dockMethodInfo;
        private MethodInfo _headsetMethodInfo;
        private MethodInfo _webcamMethodInfo;
        private MethodInfo _dongleMethodInfo;

        private ItemId _itemID;
        private ICommodity _comdity;
        private const string PenItemID = "DellPeripheral.Pen";
        private const string PenItemID0 = "DellPeripheral.Pen.0";
        private const string KeyboardItemID = "DellPeripheral.Keyboard";
        private const string KeyboardItemID0 = "DellPeripheral.Keyboard.0";

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
        public async Task<JArray> GetMouseAssignableActions(string Guid)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return new JArray(); }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_mouseInterfaceType, commodity, "AssignableActions");
                Debug.WriteLine($"{value ?? ""}");
                return value == null ? new JArray() : (JArray)value;
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
                Debug.WriteLine($"{value ?? ""}");
                return value == null ? new JArray() : (JArray)value;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
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
                Debug.WriteLine($"{value ?? ""}");
                return (bool)value;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
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
                Debug.WriteLine($"{value ?? ""}");
                return value == null ? new JArray() : (JArray)value;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
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
                Debug.WriteLine($"{value ?? ""}");
                return (string)value;
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
                Debug.WriteLine($"{value ?? ""}");
                return (bool)value;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
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
                Debug.WriteLine($"{value ?? ""}");
                return (bool)value;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {Guid} item.");
                return false;
            }
        }

        public async Task SetDpiValue(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return; }

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
        public async Task SetMouseAction(string Guid, byte[] newValue)
        {
            if (!await GetItemIDAsync("Mouse", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_mouseInterfaceType, commodity, "AssignedAction", newValue);
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
                Debug.WriteLine($"{value ?? ""}");
                return value == null ? new JArray() : (JArray)value;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
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
                Debug.WriteLine($"{value ?? ""}");
                return (bool)value;
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

            if (await GetCommodityInterfaceInstanceAsync(_mouseMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_mouseInterfaceType, commodity, "AssignableActions");
                Debug.WriteLine($"{value ?? ""}");
                return value == null ? new JArray() : (JArray)value;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
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
                    return value == null ? new JArray() : (JArray)value;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
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


        public async Task SetKbAssignKeystrokeAction(string Guid, string newValue)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return; }
            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_keyboardInterfaceType, commodity, "AssignKeystrokeAction", Encoding.UTF8.GetBytes(newValue));
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetKbAssignDialogAction(string Guid, string newValue)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_keyboardInterfaceType, commodity, "AssignDialogAction", Encoding.UTF8.GetBytes(newValue));
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetKbAssignedAction(string Guid, string newValue)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_keyboardInterfaceType, commodity, "AssignedAction", Encoding.UTF8.GetBytes(newValue));
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
            }
        }

        #endregion


        #region Webcam
        public async Task<JArray> GetPresetProfiles(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return new JArray(); }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "PresetProfiles");
                Debug.WriteLine($"{value ?? ""}");
                return value == null ? new JArray() : (JArray)value;
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
                Debug.WriteLine($"{value ?? ""}");
                return value == null ? new JArray() : (JArray)value;
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

        public async Task<string> GetCameraFirmwareVersion(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return string.Empty; }

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

        public async Task<bool> GetIsPropertyFOVSupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
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

        public async Task<int> GetFieldOfView(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
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

        public async Task<bool> GetIsPropertyHDRSupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
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

        public async Task<bool> GetIsHDROn(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
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

        public async Task<bool> CheckIsPropertyAntiFlickerSupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
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

        public async Task<int> GetAntiFlicker(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
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

        public async Task<bool> GetIsPropertyAutoFramingSupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
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

        public async Task<bool> GetIsAutoFramingOn(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
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
        public async Task<string> GetSupportedResolutions(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return string.Empty; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "SupportedResolutions");
                    return Encoding.UTF8.GetString((byte[])value);
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
                    return Encoding.UTF8.GetString((byte[])value);
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
                "Mouse" => _mouseMethodInfo,
                "Keyboard" => _keyboardMethodInfo,
                "Pen" => _penMethodInfo,
                "Webcam" => _webcamMethodInfo,
                "Headset" => _headsetMethodInfo,
                "Speaker" => _speakerMethodInfo,
                "Dongle" => _dongleMethodInfo,
                _ => null
            };
            Type interfaceType = type switch
            {
                "Mouse" => _mouseInterfaceType,
                "Keyboard" => _keyboardInterfaceType,
                "Pen" => _penInterfaceType,
                "Webcam" => _webcamInterfaceType,
                "Headset" => _headsetInterfaceType,
                "Speaker" => _speakerInterfaceType,
                "Dongle" => _dongleInterfaceType,
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
                Debug.WriteLine($"Pen Pair value: {value ?? ""}");
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
                    return value == null ? new JArray() : (JArray)value;
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
        public async Task ResetToDefault_Pen()
        {
        }

        #endregion

        #region Headset set

        public async Task<bool> SetMicNoiseCancellationAsync(string guidString, bool newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "MicNoiseCancellation", newValue);
                    writelog(" [Headset] SetMicNoiseCancellationAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetMicNoiseCancellationAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetSidetoneAsync(string guidString, bool newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "Sidetone", newValue);
                    writelog(" [Headset] SetSidetoneAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetSidetoneAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBusyLightAsync(string guidString, bool newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "BusyLight", newValue);
                    writelog(" [Headset] SetBusyLightAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetBusyLightAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetVoiceGuidanceAsync(string guidString, bool newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "VoiceGuidance", newValue);
                    writelog(" [Headset] SetVoiceGuidanceAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetVoiceGuidanceAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetSelectedPresetAsync(string guidString, int newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "SelectedPreset", newValue);
                    writelog(" [Headset] SetSelectedPresetAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetSelectedPresetAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetSidetoneLevelAsync(string guidString, int newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "SidetoneLevel", newValue);
                    writelog(" [Headset] SetSidetoneLevelAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetSidetoneLevelAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBandsGainAsync(string guidString, byte[] newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "BandsGain", newValue);
                    writelog(" [Headset] SetBandsGainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetBandsGainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAncModeAsync(string guidString, int newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "AncMode", newValue);
                    writelog(" [Headset] SetAncModeAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetAncModeAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAncGainAsync(string guidString, int newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "AncGain", newValue);
                    writelog(" [Headset] SetAncGainAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetAncGainAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetWearDetectionAsync(string guidString, int newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "WearDetection", newValue);
                    writelog(" [Headset] SetWearDetectionAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetWearDetectionAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetMicNCIncomingAsync(string guidString, bool newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "MicNCIncoming", newValue);
                    writelog(" [Headset] SetMicNCIncomingAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetMicNCIncomingAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetUnPairAsync(string guidString, bool newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "UnPair", newValue);
                    writelog(" [Headset] SetUnPairAsync Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetUnPairAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetFactoryResetAsyncValueForHeadset(string guidString, bool newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "FactoryReset", newValue);
                    writelog(" [Headset] SetFactoryResetAsyncValueForHeadset Success !");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_headsetInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($" [Headset] SetFactoryResetAsyncValueForHeadset failed: {ex.Message}");
                return false;
            }
        }

        #endregion Headset set

        #region Headset Get

        public async Task<JArray> GetDeviceItemsExAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceItemsEx");
                    writelog($"[Headset] GetDeviceItemsExAsync succeeded for {guid}");
                    return (JArray)value;
                }

                writelog($"[Headset] GetDeviceItemsExAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetDeviceItemsExAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<DeviceInterfaceType> GetInterfaceTypeAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return default;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "InterfaceType");
                    writelog($"[Headset] GetInterfaceTypeAsync succeeded for {guid}");
                    return (DeviceInterfaceType)value;
                }

                writelog($"[Headset] GetInterfaceTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return default;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetInterfaceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return default;
            }
        }

        public async Task<string> GetDeviceNameAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceName");
                    writelog($"[Headset] GetDeviceNameAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Headset] GetDeviceNameAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetDeviceNameAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetDeviceIdAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceId");
                    writelog($"[Headset] GetDeviceIdAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Headset] GetDeviceIdAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetDeviceIdAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetPluginIdAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PluginId");
                    writelog($"[Headset] GetPluginIdAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Headset] GetPluginIdAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetPluginIdAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetODMIdAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ODMId");
                    writelog($"[Headset] GetODMIdAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetODMIdAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetODMIdAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetModelNumberAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ModelNumber");
                    writelog($"[Headset] GetModelNumberAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Headset] GetModelNumberAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetModelNumberAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetInstanceNumberAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "InstanceNumber");
                    writelog($"[Headset] GetInstanceNumberAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetInstanceNumberAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetInstanceNumberAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetInstanceIdAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "InstanceId");
                    writelog($"[Headset] GetInstanceIdAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetInstanceIdAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetInstanceIdAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetFirmwareVersionAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "FirmwareVersion");
                    writelog($"[Headset] GetFirmwareVersionAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Headset] GetFirmwareVersionAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetFirmwareVersionAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetDeviceTypeAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceType");
                    writelog($"[Headset] GetDeviceTypeAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Headset] GetDeviceTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetDeviceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetParentDeviceTypeAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ParentDeviceType");
                    writelog($"[Headset] GetParentDeviceTypeAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Headset] GetParentDeviceTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetParentDeviceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GetIsBatteryLevelSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsBatteryLevelSupported");
                    writelog($"[Headset] GetIsBatteryLevelSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsBatteryLevelSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsBatteryLevelSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetBatteryLevelAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "BatteryLevel");
                    writelog($"[Headset] GetBatteryLevelAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetBatteryLevelAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetBatteryLevelAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetDeviceBatteryStatusAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceBatteryStatus");
                    writelog($"[Headset] GetDeviceBatteryStatusAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Headset] GetDeviceBatteryStatusAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetDeviceBatteryStatusAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetPairingStatusAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairingStatus");
                    writelog($"[Headset] GetPairingStatusAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Headset] GetPairingStatusAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetPairingStatusAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetMaxPairingSlotsAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "MaxPairingSlots");
                    writelog($"[Headset] GetMaxPairingSlotsAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetMaxPairingSlotsAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetMaxPairingSlotsAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetPairedDeviceCountAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairedDeviceCount");
                    writelog($"[Headset] GetPairedDeviceCountAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetPairedDeviceCountAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetPairedDeviceCountAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }
        #region Headset Get (continued)

        public async Task<int> GetTotalNumberOfPairedHostNameAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "TotalNumberOfPairedHostName");
                    writelog($"[Headset] GetTotalNumberOfPairedHostNameAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetTotalNumberOfPairedHostNameAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetTotalNumberOfPairedHostNameAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetSerialNumberAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "SerialNumber");
                    writelog($"[Headset] GetSerialNumberAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Headset] GetSerialNumberAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetSerialNumberAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GetIsReadyAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsReady");
                    writelog($"[Headset] GetIsReadyAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsReadyAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsReadyAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsDirtyAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsDirty");
                    writelog($"[Headset] GetIsDirtyAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsDirtyAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsDirtyAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsMicNoiseCancellationSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsMicNoiseCancellationSupported");
                    writelog($"[Headset] GetIsMicNoiseCancellationSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsMicNoiseCancellationSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsMicNoiseCancellationSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsSidetoneSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsSidetoneSupported");
                    writelog($"[Headset] GetIsSidetoneSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsSidetoneSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsSidetoneSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsBusyLightSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsBusyLightSupported");
                    writelog($"[Headset] GetIsBusyLightSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsBusyLightSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsBusyLightSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsVoiceGuidanceSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsVoiceGuidanceSupported");
                    writelog($"[Headset] GetIsVoiceGuidanceSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsVoiceGuidanceSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsVoiceGuidanceSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsPresetsSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsPresetsSupported");
                    writelog($"[Headset] GetIsPresetsSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsPresetsSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsPresetsSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsEqualizerSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsEqualizerSupported");
                    writelog($"[Headset] GetIsEqualizerSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsEqualizerSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsEqualizerSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<HeadsetConnectionType> GetConnectionTypeAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return HeadsetConnectionType.HeadsetConnectionTypeUnknown;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ConnectionType");
                    writelog($"[Headset] GetConnectionTypeAsync succeeded for {guid}");
                    return (HeadsetConnectionType)value;
                }

                writelog($"[Headset] GetConnectionTypeAsync failed: Could not retrieve commodity interface for {guid}");
                return HeadsetConnectionType.HeadsetConnectionTypeUnknown;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetConnectionTypeAsync failed for {guid} - Exception: {ex.Message}");
                return HeadsetConnectionType.HeadsetConnectionTypeUnknown;
            }
        }

        public async Task<bool> GetIsANCSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsANCSupported");
                    writelog($"[Headset] GetIsANCSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsANCSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsANCSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionSupported");
                    writelog($"[Headset] GetIsWearDetectionSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsWearDetectionSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsWearDetectionSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionSensitivitySupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionSensitivitySupported");
                    writelog($"[Headset] GetIsWearDetectionSensitivitySupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsWearDetectionSensitivitySupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsWearDetectionSensitivitySupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionPauseMusicSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionPauseMusicSupported");
                    writelog($"[Headset] GetIsWearDetectionPauseMusicSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsWearDetectionPauseMusicSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsWearDetectionPauseMusicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionMuteMicSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionMuteMicSupported");
                    writelog($"[Headset] GetIsWearDetectionMuteMicSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsWearDetectionMuteMicSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsWearDetectionMuteMicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionQuickPauseSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionQuickPauseSupported");
                    writelog($"[Headset] GetIsWearDetectionQuickPauseSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsWearDetectionQuickPauseSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsWearDetectionQuickPauseSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetMicNoiseCancellationAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "MicNoiseCancellation");
                    writelog($"[Headset] GetMicNoiseCancellationAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetMicNoiseCancellationAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetMicNoiseCancellationAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetMicNCIncomingAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "MicNCIncoming");
                    writelog($"[Headset] GetMicNCIncomingAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetMicNCIncomingAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetMicNCIncomingAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetSidetoneAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Sidetone");
                    writelog($"[Headset] GetSidetoneAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetSidetoneAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetSidetoneAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetBusyLightAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "BusyLight");
                    writelog($"[Headset] GetBusyLightAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetBusyLightAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetBusyLightAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetVoiceGuidanceAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "VoiceGuidance");
                    writelog($"[Headset] GetVoiceGuidanceAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetVoiceGuidanceAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetVoiceGuidanceAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetSelectedPresetAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "SelectedPreset");
                    writelog($"[Headset] GetSelectedPresetAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetSelectedPresetAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetSelectedPresetAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetSidetoneLevelAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "SidetoneLevel");
                    writelog($"[Headset] GetSidetoneLevelAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetSidetoneLevelAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetSidetoneLevelAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetMuteStatusAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "MuteStatus");
                    writelog($"[Headset] GetMuteStatusAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetMuteStatusAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetMuteStatusAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]> GetBandsGainAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "BandsGain");
                    writelog($"[Headset] GetBandsGainAsync succeeded for {guid}");
                    return (byte[])value;
                }

                writelog($"[Headset] GetBandsGainAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetBandsGainAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetAncModeAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "AncMode");
                    writelog($"[Headset] GetAncModeAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetAncModeAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetAncModeAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAncGainAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "AncGain");
                    writelog($"[Headset] GetAncGainAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetAncGainAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetAncGainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetWearDetectionAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "WearDetection");
                    writelog($"[Headset] GetWearDetectionAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[Headset] GetWearDetectionAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetWearDetectionAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetIsMicNCIncomingSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsMicNCIncomingSupported");
                    writelog($"[Headset] GetIsMicNCIncomingSupportedAsync succeeded for {guid}");
                    return (bool)value;
                }

                writelog($"[Headset] GetIsMicNCIncomingSupportedAsync failed: Could not retrieve commodity interface for {guid}");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"[Headset] GetIsMicNCIncomingSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        #endregion Headset Get

        #endregion

        #region WiredAudio


        public async Task<bool> SetBassAsync(string guid, int newValue)
        {
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

        public async Task<bool> SetMidRangeAsync(string guid, int newValue)
        {
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

        public async Task<bool> SetTrebleAsync(string guid, int newValue)
        {
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

        public async Task<bool> SetProfileForSpeaker(string guid, string newValue)
        {
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

        public async Task<bool> SetIsWiredAudioMicMuteSoundEnableAsync(string guid, bool newValue)
        {
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

        public async Task<bool> SetWiredAudioVolumeAdjustmentToneAsync(string guid, int newValue)
        {
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

        public async Task<bool> SetIsWiredAudioIMicNSEnableAsync(string guid, bool newValue)
        {
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

        public async Task<bool> SetResetToDefaultAsyncForSoundbar(string guid, bool newValue)
        {
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

        public async Task<string> GetProfileAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "Profile");
                    writelog($"[Speaker] GetProfileAsync succeeded for {guid}");
                    return (string)value;
                }

                writelog($"[Speaker] GetProfileAsync failed: Could not retrieve commodity interface for {guid}");
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Speaker] GetTrebleAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetBassAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "Bass");
                    writelog($"[Speaker] GetBassAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<int> GetMidRangeAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "MidRange");
                    writelog($"[Speaker] GetMidRangeAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<int> GetTrebleAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "Treble");
                    writelog($"[Speaker] GetTrebleAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<bool> GetIsWiredAudioMicMuteSoundEnableAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsWiredAudioMicMuteSoundEnable");
                    writelog($"[Speaker] GetIsWiredAudioMicMuteSoundEnableAsync succeeded for {guid}");
                    return (bool)value;
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

        public async Task<int> GetWiredAudioVolumeAdjustmentToneAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "WiredAudioVolumeAdjustmentTone");
                    writelog($"[Speaker] GetWiredAudioVolumeAdjustmentToneAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<bool> GetIsWiredAudioIMicNSEnableAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "IsWiredAudioIMicNSEnable");
                    writelog($"[Speaker] GetIsWiredAudioIMicNSEnableAsync succeeded for {guid}");
                    return (bool)value;
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

        public async Task<bool> GetIsAudioEqualizerSupportedAsync(string guid)
        {
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
                    writelog($"[Speaker] GetIsAudioEqualizerSupportedAsync succeeded for {guid}");
                    return (bool)value;
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

        #endregion

        #region Dongle

        public async Task<string> GetFirmwareVersionAsyncForDongle(string guid)
        {
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
                    return (string)value;
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

        public async Task<string> GetConnectedDeviceInfoAsyncForDongle(string guid)
        {
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
                    return (string)value;
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

        public async Task<string> GetDeviceIdAsyncForDongle(string guid)
        {
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
                    return (string)value;
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

        public async Task<string> GetPluginIdAsyncForDongle(string guid)
        {
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
                    return (string)value;
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

        public async Task<JArray> GetDeviceItemsExAsyncForDongle(string guid)
        {
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
                    return (JArray)value;
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
                Debug.WriteLine($"\nError handling {methodInfo.MemberType}'s {_itemID} item.\n{ex}");
                writelog($"\nError handling {methodInfo.MemberType}'s {_itemID} item.\n{ex}");
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
                    await _commSdk.InitializeAsync(appId, new CancellationTokenSource().Token);

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
                    _headsetcom.AncModeChanged += _headsetcomdity_AncModeChange;
                    _headsetcom.Connected += _comdity_Connected;
                    _headsetcom.Disconnected += _comdity_Disconnected;
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
                    //_speakercom.IsIMicNSEnabledChanged += _speakercomdity_IsIMicNSEnabledChanged;
                    //_speakercom.VolumeAdjustmentToneChanged += _speakercomdity_VolumeAdjustmentToneChanged;
                    //_speakercom.IsMicMuteSoundEnabledChanged += _speakercomdity_IsMicMuteSoundEnabledChanged;
                    //_speakercom.MuteStatusChanged += _speakercomdity_IsMuteStatusChanged;
                    //EventHandler<MuteStatusChangedArgs> MuteStatusChanged;
                    //AddMuteStatusChangedEventAsync
                    _speakercom.Connected += _comdity_Connected;
                    _speakercom.Disconnected += _comdity_Disconnected;
                    _speakercom.MuteStatusChanged += _speakercomdity_IsMuteStatusChanged;
                    writelog($"Speaker Commodity event registered");
                }
                catch (Exception e)
                {
                    writelog($"Find ISpeakerCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
            }

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
                Debug.WriteLine($"commodity: {commodity.GetType().Name} Property: {property}");
                var obj = interfaceType.GetProperty(property).GetGetMethod().Invoke(commodity, null);
                Debug.WriteLine($"{obj.ToString()}");
                return obj;
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
            Debug.WriteLine($"ItemID: {_itemID}; Type: {interfaceType.Name}; property: {property}; value: {value ?? ""}");
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

        #region Headset Event

        private void _headsetcomdity_IsReadyChanged(object sender, IsReadyChangedArgs e)
        {
            Debug.WriteLine($"[Headset] IsReadyChanged {e.IsReady} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
            writelog($"[Headset] IsReadyChanged {e.IsReady} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
        }
        private void _headsetcomdity_FirmwareVersionChanged(object sender, FirmwareVersionChangedArgs e)
        {
            Debug.WriteLine($"[Headset]FirmwareVersionChanged {e.FirmwareVersion} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
            writelog($"[Headset]FirmwareVersionChanged {e.FirmwareVersion} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
        }
        private void _headsetcomdity_AncModeChange(object sender, AncModeChangedArgs e)
        {
            Debug.WriteLine($"[Headset]AncModeChange {e.AncMode} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
            writelog($"[Headset]AncModeChange {e.AncMode} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
        }
        #endregion

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
    }
}