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
        private const string publisherCompany = "Dell Inc.";
        private const string publisherWebsite = "https://www.dell.com";
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
        private const string WebcamItemID = "DellPeripheral.Webcam";
        private const string HeadsetItemID = "DellPeripheral.Headset";
        private bool IsDTPReady = false;

        public const string PluginLogId = "DTPProxy";


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
            writelog($"Initialized successfully");
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
        public void OnUIUpdateNotify(UpdateUINotify e)
        {
            DTPEventHandler?.Invoke(this, e);
        }

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

                    model = SACommonHelper.MappingModel(model);
                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\Actions\{model}.json");
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            writelog($"[DTPProxyPlugin] [RestoreToDefaultPen] Delete setting file failed: {ex}");
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
        public async Task<string> GetKeyboardKeystrokeDisplayData(string Guid)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return string.Empty; }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "KeystrokeDisplayData");
                Debug.WriteLine($"{value ?? ""}");
                return (string)value;
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {Guid} item.");
                return string.Empty;
            }
        }
        public async Task<bool> StartKeyboardKeystrokeRecording(string Guid)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "StartKeystrokeRecording");
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
        public async Task<bool> StopKeyboardKeystrokeRecording(string Guid)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_keyboardInterfaceType, commodity, "StopKeystrokeRecording");
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


        public async Task SetKbAssignKeystrokeAction(string Guid, byte[] newValue)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return; }
            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_keyboardInterfaceType, commodity, "AssignKeystrokeAction", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetKbAssignDialogAction(string Guid, byte[] newValue)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_keyboardInterfaceType, commodity, "AssignDialogAction", newValue);
            }
            else
            {
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_keyboardInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetKbAssignedAction(string Guid, byte[] newValue)
        {
            if (!await GetItemIDAsync("Keyboard", Guid))
            { return; }

            if (await GetCommodityInterfaceInstanceAsync(_keyboardMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_keyboardInterfaceType, commodity, "AssignedAction", newValue);
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

            model = SACommonHelper.MappingModel(model);
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\Actions\{model}.json");
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    writelog($"[DTPProxyPlugin] [RestoreToDefaultPen] Delete setting file failed: {ex}");
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
                    return value == null ? new JArray() : (JArray)value;
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
        public async Task<bool> GetIsWindowsHelloCapabilityVerified(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsWindowsHelloCapabilityVerified");
                    return (bool)value;
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
                    return (bool)value;
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
        public async Task<bool> GeIsPropertyAntiFlickerSupported(string Guid)
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

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "AntiFlicker");
                return (int)value;
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
        public async Task<bool> GetIsESISupported(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsESISupported");
                    return (bool)value;
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
        public async Task<int> GetZoom(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return -1; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "Zoom");
                return value == null ? -1 : (int)value;
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
                return value == null ? -1 : (int)value;
            }
            else
            {
                Debug.WriteLine($"[GetFocus]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetFocus]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return -1;
            }
        }
        public async Task<bool> GetIsFocusOn(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
            {
                var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsFocusOn");
                return value == null ? false : (bool)value;
            }
            else
            {
                Debug.WriteLine($"[GetIsFocusOn]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetIsFocusOn]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
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
                return value == null ? -1 : (int)value;
            }
            else
            {
                Debug.WriteLine($"[GetPriority]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"[GetPriority]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                return -1;
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
        public async Task<bool> SetZoom(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

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
        public async Task<bool> SetIsAutoFramingOn(string Guid, bool newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

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
        public async Task<bool> SetFieldOfView(string Guid, int newValue)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

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
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
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
                Debug.WriteLine($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
                writelog($"Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {Guid} item.");
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

        public async Task<bool?> GetIsPrioritizeExternalWebcam(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return null; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "IsPrioritizeExternalWebcam");
                    return (bool?)value;
                }
                else
                {
                    Debug.WriteLine($"[GetIsPrioritizeExternalWebcam]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    writelog($"[GetIsPrioritizeExternalWebcam]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return null;
                }
            }
            else
            {
                Debug.WriteLine($"[GetIsPrioritizeExternalWebcam]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                writelog($"[GetIsPrioritizeExternalWebcam]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return null;
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
                    return (bool)value;
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
        public async Task<bool> GetZoomMeetingType(string Guid)
        {
            if (!await GetItemIDAsync("Webcam", Guid))
            { return false; }

            if (_webcamMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_webcamMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_webcamInterfaceType, commodity, "ZoomMeetingType");
                    return (bool)value;
                }
                else
                {
                    writelog($"[GetZoomMeetingType]Could not retrieve the Commodity Interface {_webcamInterfaceType} for the {_itemID} item.");
                    return false;
                }
            }
            else
            {
                writelog($"[GetZoomMeetingType]Could not retrieve the Commodity Interface for the {_itemID} item. _webcamMethodInfo is null");
                return false;
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
                    return (bool)value;
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
                "Mouse" => _mouseMethodInfo,
                "Keyboard" => _keyboardMethodInfo,
                "Pen" => _penMethodInfo,
                "Webcam" => _webcamMethodInfo,
                "Headset" => _headsetMethodInfo,
                "Speaker" => _speakerMethodInfo,
                "Dongle" => _dongleMethodInfo,
                "Dock" => _dockMethodInfo,
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
                "Dock" => _dockInterfaceType,
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
                Debug.WriteLine($"{_itemID}");
                Trace.WriteLine($"{_itemID}");
                if (await GetCommodityInterfaceInstanceAsync(methodInfo) is ICommodity commodity)
                {
                    Debug.WriteLine($"{commodity.GetType}");
                    var value = GetPropertyValue(interfaceType, commodity, "DeviceId");
                    if (value == null)
                    {
                        Debug.WriteLine($"Not found {type} GUID: {guid}");
                        writelog($"Not found {type} GUID: {guid}");
                        return false;
                    }
                    Debug.WriteLine((string)value);
                    if ((string)value == guid)
                    { return true; }
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
        public async Task<bool> StartKeyCapturePen()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "StartKeyCapture");
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
        public async Task<bool> FinishKeyCapturePen()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "FinishKeyCapture");
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
        public async Task<string> KeyCaptureData()
        {
            _itemID = new ItemId(PenItemID0);

            if (_penMethodInfo != null)
            {
                if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
                {
                    var value = GetPropertyValue(_penInterfaceType, commodity, "KeyCaptureData");
                    return (string)value;
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
                Debug.WriteLine($"[GetIsSideBottomButtonHoverClick]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
                writelog($"[GetIsSideBottomButtonHoverClick]Could not retrieve the Commodity Interface for the {_itemID} item. _penMethodInfo is null");
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
            _itemID = new ItemId(PenItemID0);

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
            await SetTiltSensitivity(PenItemID0, 1);
            await SetIsSideTopButtonHoverClick(PenItemID0, false);
            await SetIsSideBottomButtonHoverClick(PenItemID0, false);

            await RestoreRadialMenuToDefault();

            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\Actions\pen.json");
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    writelog($"[DTPProxyPlugin] [RestoreToDefaultPen] Delete setting file failed: {ex}");
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
        public async Task<bool> SetMicNoiseCancellationAsync(string guidString, bool newValue)
        {
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

        public async Task<bool> SetSidetoneAsync(string guidString, bool newValue)
        {
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

        public async Task<bool> SetBusyLightAsync(string guidString, bool newValue)
        {
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

        public async Task<bool> SetVoiceGuidanceAsync(string guidString, bool newValue)
        {
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

        public async Task<bool> SetSelectedPresetAsync(string guidString, int newValue)
        {
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

        public async Task<bool> SetSidetoneLevelAsync(string guidString, int newValue)
        {
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

        public async Task<bool> SetBandsGainAsync(string guidString, byte[] newValue)
        {
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

        public async Task<bool> SetBand1GainAsync(string guidString, int newValue)
        {
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

        public async Task<bool> SetBand2GainAsync(string guidString, int newValue)
        {
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

        public async Task<bool> SetBand3GainAsync(string guidString, int newValue)
        {
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

        public async Task<bool> SetBand4GainAsync(string guidString, int newValue)
        {
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

        public async Task<bool> SetBand5GainAsync(string guidString, int newValue)
        {
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
        public async Task<bool> SetAncModeAsync(string guidString, int newValue)
        {
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

        public async Task<bool> SetAncGainAsync(string guidString, int newValue)
        {
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

        public async Task<bool> SetWearDetectionAsync(string guidString, bool newValue)
        {
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

        public async Task<bool> SetIsWearDetectionMuteMicEnabledAsync(string guidString, bool newValue)
        {
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

        public async Task<bool> SetIsWearDetectionPauseMusicEnabledAsync(string guidString, bool newValue)
        {
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

        public async Task<bool> SetWearDetectionQuickPauseAsync(string guidString, int newValue)
        {
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

        public async Task<bool> SetWearDetectionSensitivityAsync(string guidString, int newValue)
        {
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

        public async Task<bool> SetMicNCIncomingAsync(string guidString, bool newValue)
        {
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

        public async Task<bool> SetUnPairAsync(string guidString, bool newValue)
        {
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

        public async Task<bool> SetFactoryResetAsyncValueForHeadset(string guidString, bool newValue)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guidString))
                    return false;

                if (await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo) is ICommodity commodity)
                {
                    SetPropertyValue(_headsetInterfaceType, commodity, "FactoryReset", newValue);
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

        public async Task<bool> SetBoomMicAsync(string guidString, bool newValue)
        {
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
                        return (JArray)value;
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

        public async Task<DeviceInterfaceType> GetHeadsetInterfaceTypeAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return default;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "InterfaceType");
                    writelog($"[DTPProxyPlugin] [Headset] GetInterfaceTypeAsync succeeded for {guid}");
                    return (DeviceInterfaceType)value;
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

        public async Task<string> GetHeadsetDeviceNameAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceName");
                    writelog($"[DTPProxyPlugin] [Headset] GetDeviceNameAsync succeeded for {guid}");
                    return (string)value;
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

        public async Task<string> GetHeadsetDeviceIdAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceId");
                    writelog($"[DTPProxyPlugin] [Headset] GetDeviceIdAsync succeeded for {guid}");
                    return (string)value;
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

        public async Task<string> GetHeadsetPluginIdAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PluginId");
                    writelog($"[DTPProxyPlugin] [Headset] GetPluginIdAsync succeeded for {guid}");
                    return (string)value;
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

        public async Task<int> GetHeadsetODMIdAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ODMId");
                    writelog($"[DTPProxyPlugin] [Headset] GetODMIdAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<string> GetHeadsetModelNumberAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ModelNumber");
                    writelog($"[DTPProxyPlugin] [Headset] GetModelNumberAsync succeeded for {guid}");
                    return (string)value;
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

        public async Task<int> GetHeadsetInstanceNumberAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "InstanceNumber");
                    writelog($"[DTPProxyPlugin] [Headset] GetInstanceNumberAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<int> GetHeadsetInstanceIdAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "InstanceId");
                    writelog($"[DTPProxyPlugin] [Headset] GetInstanceIdAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<string> GetHeadsetFirmwareVersionAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "FirmwareVersion");
                    writelog($"[DTPProxyPlugin] [Headset] GetFirmwareVersionAsync succeeded for {guid}");
                    return (string)value;
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

        public async Task<string> GetHeadsetDeviceTypeAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceType");
                    writelog($"[DTPProxyPlugin] [Headset] GetDeviceTypeAsync succeeded for {guid}");
                    return (string)value;
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

        public async Task<string> GetHeadsetParentDeviceTypeAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "ParentDeviceType");
                    writelog($"[DTPProxyPlugin] [Headset] GetParentDeviceTypeAsync succeeded for {guid}");
                    return (string)value;
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

        public async Task<bool> GetHeadsetIsBatteryLevelSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsBatteryLevelSupported");
                    writelog($"[DTPProxyPlugin] [Headset] GetIsBatteryLevelSupportedAsync succeeded for {guid}");
                    return (bool)value;
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

        public async Task<int> GetHeadsetBatteryLevelAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "BatteryLevel");
                    writelog($"[DTPProxyPlugin] [Headset] GetBatteryLevelAsync succeeded for {guid}");
                    return (int)value;
                }

                writelog($"[DTPProxyPlugin] [Headset] GetBatteryLevelAsync failed: Could not retrieve commodity interface for {guid}");
                return -1;
            }
            catch (Exception ex)
            {
                writelog($"[DTPProxyPlugin] [Headset] GetBatteryLevelAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetHeadsetDeviceBatteryStatusAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "DeviceBatteryStatus");
                    writelog($"[DTPProxyPlugin] [Headset] GetDeviceBatteryStatusAsync succeeded for {guid}");
                    return (string)value;
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

        public async Task<string> GetHeadsetPairingStatusAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairingStatus");
                    writelog($"[DTPProxyPlugin] [Headset] GetPairingStatusAsync succeeded for {guid}");
                    return (string)value;
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

        public async Task<string> GetHeadsetPairedHostName1Async(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairedHostName1");
                    writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName1Async succeeded for {guid}");
                    return (string)value;
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

        public async Task<string> GetHeadsetPairedHostName2Async(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairedHostName2");
                    writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName2Async succeeded for {guid}");
                    return (string)value;
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

        public async Task<string> GetHeadsetPairedHostName3Async(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairedHostName3");
                    writelog($"[DTPProxyPlugin] [Headset] GetPairedHostName3Async succeeded for {guid}");
                    return (string)value;
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

        public async Task<int> GetHeadsetMaxPairingSlotsAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "MaxPairingSlots");
                    writelog($"[DTPProxyPlugin] [Headset] GetMaxPairingSlotsAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<int> GetHeadsetPairedDeviceCountAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "PairedDeviceCount");
                    writelog($"[DTPProxyPlugin] [Headset] GetPairedDeviceCountAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<int> GetHeadsetTotalNumberOfPairedHostNameAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "TotalNumberOfPairedHostName");
                    writelog($"[DTPProxyPlugin] [Headset] GetTotalNumberOfPairedHostNameAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<string> GetHeadsetSerialNumberAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return null;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "SerialNumber");
                    writelog($"[DTPProxyPlugin] [Headset] GetSerialNumberAsync succeeded for {guid}");
                    return (string)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsReadyAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsDirtyAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsMicNoiseCancellationSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsSidetoneSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsBusyLightSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsVoiceGuidanceSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsPresetsSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsEqualizerSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetConnectionTypeAsync succeeded for {guid}");
                    return (HeadsetConnectionType)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsANCSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionSensitivitySupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionQuickPauseSupportedAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetMicNoiseCancellationAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetMicNCIncomingAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetSidetoneAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetBusyLightAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetVoiceGuidanceAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetSelectedPresetAsync succeeded for {guid}");
                    return (int)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetSidetoneLevelAsync succeeded for {guid}");
                    return (int)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetMuteStatusAsync succeeded for {guid}");
                    return (bool)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetBandsGainAsync succeeded for {guid}");
                    return (byte[])value;
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

        public async Task<int> GetBand1GainAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Band1Gain");
                    writelog($"[DTPProxyPlugin] [Headset] GetBand1GainAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<int> GetBand2GainAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Band2Gain");
                    writelog($"[DTPProxyPlugin] [Headset] GetBand2GainAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<int> GetBand3GainAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Band3Gain");
                    writelog($"[DTPProxyPlugin] [Headset] GetBand3GainAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<int> GetBand4GainAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Band4Gain");
                    writelog($"[DTPProxyPlugin] [Headset] GetBand4GainAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<int> GetBand5GainAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "Band5Gain");
                    writelog($"[DTPProxyPlugin] [Headset] GetBand5GainAsync succeeded for {guid}");
                    return (int)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetAncModeAsync succeeded for {guid}");
                    return (int)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetAncGainAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<bool> GetWearDetectionAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "WearDetection");
                    writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionAsync succeeded for {guid}");
                    return (bool)value;
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

        public async Task<bool> GetIsWearDetectionPauseMusicEnabledAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionPauseMusicEnabled");
                    writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionPauseMusicEnabledAsync succeeded for {guid}");
                    return (bool)value;
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

        public async Task<bool> GetIsWearDetectionMuteMicEnabledAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsWearDetectionMuteMicEnabled");
                    writelog($"[DTPProxyPlugin] [Headset] GetIsWearDetectionMuteMicEnabledAsync succeeded for {guid}");
                    return (bool)value;
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

        public async Task<int> GetWearDetectionSensitivityAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "WearDetectionSensitivity");
                    writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionSensitivityAsync succeeded for {guid}");
                    return (int)value;
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

        public async Task<int> GetWearDetectionQuickPauseAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return -1;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "WearDetectionQuickPause");
                    writelog($"[DTPProxyPlugin] [Headset] GetWearDetectionQuickPauseAsync succeeded for {guid}");
                    return (int)value;
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
                    writelog($"[DTPProxyPlugin] [Headset] GetIsMicNCIncomingSupportedAsync succeeded for {guid}");
                    return (bool)value;
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

        public async Task<bool> GetIsBoomMicSupportedAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "IsBoomMicSupported");
                    writelog($"[DTPProxyPlugin] [Headset] GetIsBoomMicSupportedAsync succeeded for {guid}");
                    return (bool)value;
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

        public async Task<bool> GetBoomMicAsync(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Headset", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_headsetMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_headsetInterfaceType, commodity, "BoomMic");
                    writelog($"[DTPProxyPlugin] [Headset] GetBoomMicAsync succeeded for {guid}");
                    return (bool)value;
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
        private async Task RegisterEventsForAllHeadsetAsync()
        {
            var headsets = await GetHeadsetDeviceItemsExAsync();

            if (headsets != null && headsets.Count > 0)
            {
                writelog($"Headset instance count: {headsets} to register");

                for (int i = 0; i < headsets.Count; i++)
                {
                    bool result = await RegisterEventsForHeadsetAsync(i);

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
                //try to force release current --> will catch exception  1123
                //await UnregisterEventsForWebcamAsync(0);
            }
        }

        private async Task UnregisterEventsForAllHeadsetAsync()
        {
            var headsets = await GetHeadsetDeviceItemsExAsync();

            if (headsets.Count > 0)
            {
                writelog($"[Headset] instance count: {headsets} to unregister.");

                for (int i = headsets.Count - 1; i >= 0; i--)
                {
                    bool result = await UnregisterEventsForHeadsetAsync(i);
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
                    _Headsetcom.WearDetectionChanged += Headset_WearDetectionChanged;
                    _Headsetcom.WearDetectionSensitivityChanged += Headset_WearDetectionSensitivityChanged;
                    _Headsetcom.IsWearDetectionPauseMusicEnabledChanged += Headset_IsWearDetectionPauseMusicEnabledChanged;
                    _Headsetcom.IsWearDetectionMuteMicEnabledChanged += Headset_IsWearDetectionMuteMicEnabledChanged;
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
                _comdity = await _commSdk.GetCommodityAsync<IHeadsetCommodity>(new ItemId($"DellPeripheral.Webcam.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity _Headsetcom)
                {
                    _Headsetcom.WearDetectionChanged -= Headset_WearDetectionChanged;
                    _Headsetcom.WearDetectionSensitivityChanged -= Headset_WearDetectionSensitivityChanged;
                    _Headsetcom.IsWearDetectionPauseMusicEnabledChanged -= Headset_IsWearDetectionPauseMusicEnabledChanged;
                    _Headsetcom.IsWearDetectionMuteMicEnabledChanged -= Headset_IsWearDetectionMuteMicEnabledChanged;
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

        private string CreateHeadsetEventMsg(string devType, string eventType, string devID, string eventContent = "NewValue:NoContent")
        {
            writelog($"[Headset] Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}");
            return $"HeadsetEvent_5;Device:{devType};EventType:{eventType};DeviceId:{devID};{eventContent}";
        }

        private void SendHeadsetEventToUI(string sendMsg)
        {
            UpdateUINotify headsetEventNotify = new UpdateUINotify();
            headsetEventNotify.UI_Field_Name = $"{sendMsg}";
            OnUIUpdateNotify(headsetEventNotify);
        }

        #endregion Headset Event

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

        public async Task<bool> GetMuteStatusAsyncForSpeaker(string guid)
        {
            try
            {
                if (!await GetItemIDAsync("Speaker", guid))
                    return false;

                var commodity = await GetCommodityInterfaceInstanceAsync(_speakerMethodInfo);
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_speakerInterfaceType, commodity, "MuteStatus");
                    writelog($"[Speaker] GetMuteStatusAsyncForSpeaker succeeded for {guid}");
                    return (bool)value;
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

        #region Dock
        public Task<DockData> GetDockData(string guid)
        {
            try
            {
                if (!GetItemIDAsync("Dock", guid).Result)
                {
                    writelog(" [Dock] Failed to retrieve guid.");
                    return null;
                }
                var commodity = GetCommodityInterfaceInstanceAsync(_dockMethodInfo).Result;
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
                                                return Task.FromResult(dockData);
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
                return null;
            }
            catch (Exception ex)
            {
                writelog($"[Dock] GetDockData failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }
        public Task<string> GetFirmwareVersionForDock(string guid)
        {
            try
            {
                if (!GetItemIDAsync("Dock", guid).Result)
                {
                    writelog(" [Dock] Failed to retrieve guid.");
                    return Task.FromResult("");
                }
                var commodity = GetCommodityInterfaceInstanceAsync(_dockMethodInfo).Result;
                if (commodity is ICommodity)
                {
                    var value = GetPropertyValue(_dockInterfaceType, commodity, "FirmwareVersion");
                    writelog($"[Dock] GetFirmwareVersionForDock succeeded for {guid}");
                    writelog($"[Dock] GetDockServiceTagForDock succeeded for {(string)value}");
                    return Task.FromResult((string)value);
                }

                writelog($"[Dock] GetFirmwareVersionForDock failed: Could not retrieve commodity interface for {guid}");
                return Task.FromResult("");
            }
            catch (Exception ex)
            {
                writelog($"[Dock] GetFirmwareVersionForDock failed for {guid} - Exception: {ex.Message}");
                return Task.FromResult("");
            }
        }
        public Task<string> GetDockServiceTagForDock(string guid)
        {
            try
            {
                if (!GetItemIDAsync("Dock", guid).Result)
                {
                    writelog(" [Dock] Failed to retrieve guid.");
                    return Task.FromResult("");
                }
                var commodity = GetCommodityInterfaceInstanceAsync(_dockMethodInfo).Result;
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
                                        return Task.FromResult(payloadElement);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                writelog($"[Dock] GetDockServiceTagForDock Error : {ex.Message}");
                            }
                        }
                    }
                    return Task.FromResult("");
                }

                writelog($"[Dock] GetDockServiceTagForDock failed: Could not retrieve commodity interface for {guid}");
                return Task.FromResult("");
            }
            catch (Exception ex)
            {
                writelog($"[Dock] GetDockServiceTagForDock failed for {guid} - Exception: {ex.Message}");
                return Task.FromResult("");
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
        private void writelog(string text,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
            log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[DTPProxyPlugin] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
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

            try
            {
                if (_commSdk != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _commSdk.InitializeAsync(appId, new CancellationTokenSource().Token);

                        IsDTPReady = true;
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
            catch (Exception e)
            {
                writelog($"Catch exception[{e.Message}] in InitializeDTPProxy function");
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

            writelog($"Register Commodity event...");
            _comdity = await _commSdk.GetCommodityAsync<IHeadsetCommodity>(new ItemId("DellPeripheral.Headset"), CancellationToken.None);
            if (_comdity is Dell.TechHub.Commodity.Peripheral.IHeadsetCommodity _headsetcom)
            {
                try
                {
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
            writelog($"Register Webcam Commodity event...");
            _comdity = await _commSdk.GetCommodityAsync<IWebcamCommodity>(new ItemId("DellPeripheral.Webcam"), CancellationToken.None);
            if (_comdity is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity _WebcamComConnectEvent)
            {
                try
                {
                    //PrintWebcamObjectInfo(_WebcamComConnectEvent);

                    _WebcamComConnectEvent.Connected += Webcam_Connected;
                    _WebcamComConnectEvent.Disconnected += Webcam_Disconnected;

                    writelog($"Webcam Commodity event(connected/disconnected) registered");
                }
                catch (Exception e)
                {
                    writelog($"Find IWebcamCommodity not find  time: {DateTime.Now.ToString("hh.mm.ss.ffffff") + " Message: " + e.Message}");
                }
            }

            await RegisterEventsForAllWebcamsAsync();

            await RegisterEventsForAllHeadsetAsync();
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

        private async Task RegisterEventsForAllWebcamsAsync()
        {
            var webcams = await GetWebcamDevsCountAsync();

            if (webcams > 0)
            {
                writelog($"Webcam instance count: {webcams} to register");

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
            }
            else
            {
                writelog($"No any webcam instance to register.");
                //try to force release current --> will catch exception  1123
                //await UnregisterEventsForWebcamAsync(0);
            }
        }

        private async Task UnregisterEventsForAllWebcamsAsync()
        {
            var webcams = await GetWebcamDevsCountAsync();

            if (webcams > 0)
            {
                writelog($"Webcam instance count: {webcams} to unregister.");

                for (int i = webcams - 1; i >= 0; i--)
                {
                    bool result = await UnregisterEventsForWebcamAsync(i);
                }
            }
            else
                writelog($"No any webcam instance to unregister.");
        }

        private async Task<bool> RegisterEventsForWebcamAsync(int index)
        {
            if (null == _commSdk || null == _comdity || index < 0)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<IWebcamCommodity>(new ItemId($"DellPeripheral.Webcam.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity _Webcamcom)
                {
                    //PrintWebcamObjectInfo(_Webcamcom);

                    _Webcamcom.ProfileManagerAdded += Webcam_ProfileManagerAdded;
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
                    _Webcamcom.IsHDROnChanged += Webcam_IsHDROnChanged;
                    _Webcamcom.SerialNumberChanged += Webcam_SerialNumberChanged;
                    _Webcamcom.IsZoomMeetingActiveChanged += Webcam_IsZoomMeetingActiveChanged; //for QAM
                    _Webcamcom.IsZoomScreenShareActiveChanged += Webcam_IsZoomScreenShareActiveChanged; //for QAM
                    _Webcamcom.ZoomMeetingTypeChanged += Webcam_ZoomMeetingTypeChanged; //for QAM

                    _Webcamcom.WALSnoozeTimeLeftInSecondsChanged += Webcam_WALSnoozeTimeLeftInSecondsChanged;
                    _Webcamcom.Esi_IsWALLockCountdownStartedChanged += Webcam_Esi_IsWALLockCountdownStartedChanged;
                    _Webcamcom.Esi_IsCameraSensorCoveredChanged += Webcam_Esi_IsCameraSensorCoveredChanged;
                    _Webcamcom.Esi_WALLockCountdownChanged += Webcam_Esi_WALLockCountdownChanged;

                    writelog($"Webcam{index} Commodity events registered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"Webcam{index} RegisterEventsForWebcam Exception {e.Message}");

                return false;
            }

            return false;
        }

        private async Task<bool> UnregisterEventsForWebcamAsync(int index)
        {
            if (null == _commSdk || null == _comdity || index < 0)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<IWebcamCommodity>(new ItemId($"DellPeripheral.Webcam.{index}"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity _Webcamcom)
                {
                    _Webcamcom.ProfileManagerAdded -= Webcam_ProfileManagerAdded;
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
                    _Webcamcom.IsHDROnChanged -= Webcam_IsHDROnChanged;
                    _Webcamcom.SerialNumberChanged -= Webcam_SerialNumberChanged;
                    _Webcamcom.IsZoomMeetingActiveChanged -= Webcam_IsZoomMeetingActiveChanged;
                    _Webcamcom.IsZoomScreenShareActiveChanged -= Webcam_IsZoomScreenShareActiveChanged;

                    _Webcamcom.WALSnoozeTimeLeftInSecondsChanged -= Webcam_WALSnoozeTimeLeftInSecondsChanged;
                    _Webcamcom.Esi_IsWALLockCountdownStartedChanged -= Webcam_Esi_IsWALLockCountdownStartedChanged;
                    _Webcamcom.Esi_IsCameraSensorCoveredChanged -= Webcam_Esi_IsCameraSensorCoveredChanged;
                    _Webcamcom.Esi_WALLockCountdownChanged -= Webcam_Esi_WALLockCountdownChanged;

                    writelog($"Webcam{index} Commodity events unregistered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"Webcam{index} UnregisterEventsForWebcam Exception {e.Message}");

                return false;
            }

            return false;
        }

        private async Task<bool> UnregisterEventsForWebcamAsync()
        {
            if (null == _commSdk || null == _comdity)
                return false;

            try
            {
                _comdity = await _commSdk.GetCommodityAsync<IWebcamCommodity>(new ItemId($"DellPeripheral.Webcam"), CancellationToken.None);

                if (_comdity is Dell.TechHub.Commodity.Peripheral.IWebcamCommodity _Webcamcom)
                {
                    _Webcamcom.ProfileManagerAdded -= Webcam_ProfileManagerAdded;
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
                    _Webcamcom.IsHDROnChanged -= Webcam_IsHDROnChanged;
                    _Webcamcom.SerialNumberChanged -= Webcam_SerialNumberChanged;
                    _Webcamcom.IsZoomMeetingActiveChanged -= Webcam_IsZoomMeetingActiveChanged;
                    _Webcamcom.IsZoomScreenShareActiveChanged -= Webcam_IsZoomScreenShareActiveChanged;

                    _Webcamcom.WALSnoozeTimeLeftInSecondsChanged -= Webcam_WALSnoozeTimeLeftInSecondsChanged;
                    _Webcamcom.Esi_IsWALLockCountdownStartedChanged -= Webcam_Esi_IsWALLockCountdownStartedChanged;
                    _Webcamcom.Esi_IsCameraSensorCoveredChanged -= Webcam_Esi_IsCameraSensorCoveredChanged;
                    _Webcamcom.Esi_WALLockCountdownChanged -= Webcam_Esi_WALLockCountdownChanged;

                    writelog($"Webcam Commodity events unregistered successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                writelog($"Webcam UnregisterEventsForWebcam Exception {e.Message}");

                return false;
            }

            return false;
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
            if (!IsDTPReady)
                return null;

            try
            {
                writelog($"commodity: {commodity.GetType().Name} Property: {property}");
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

        private bool SetPropertyValue(Type interfaceType, ICommodity commodity, string property, object value)
        {
            Debug.WriteLine($"ItemID: {_itemID}; Type: {interfaceType.Name}; property: {property}; value: {value ?? ""}");
            if (!IsDTPReady)
                return false;

            try
            {
                interfaceType.GetProperty(property).GetSetMethod().Invoke(commodity, new[] { value });
                return true;
            }
            catch (Exception ex)
            {
                writelog($"Error while setting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
                return false;
            }
        }

        private bool SetPropertyValue(Type interfaceType, ICommodity commodity, string property, byte[] value)
        {
            Debug.WriteLine($"ItemID: {_itemID}; Type: {interfaceType.Name}; property: {property}; value: {Encoding.UTF8.GetString(value)}");
            if (!IsDTPReady)
                return false;

            try
            {
                interfaceType.GetProperty(property).GetSetMethod().Invoke(commodity, new[] { value });
                return true;
            }
            catch (Exception ex)
            {
                writelog($"Error while setting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
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

        #region Headset Event

        private void _headsetcomdity_IsReadyChanged(object sender, IsReadyChangedArgs e)
        {
            Debug.WriteLine($"[DTPProxyPlugin] [Headset] IsReadyChanged {e.IsReady} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
            writelog($"[DTPProxyPlugin] [Headset] IsReadyChanged {e.IsReady} Changed for Device ID: {e.DeviceId}  !!!!!!!!!!!!!!!");
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


        #region Webcam event
        //webcam register condition
        //1. some devices has connected before DTPPlugin init
        //   A. register events for them, and remove events when disconnected
        //2. new devices connected --> register

        private void Webcam_ZoomMeetingTypeChanged(object sender, ZoomMeetingTypeChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_ZoomMeetingTypeChanged",
                                    e.DeviceId, $"NewValue:{e.ZoomMeetingType}"));

            writelog($"Catch event Webcam_ZoomMeetingTypeChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_SharpnessChanged(object sender, SharpnessChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_SharpnessChanged",
                                    e.DeviceId, $"NewValue:{e.Sharpness}"));

            writelog($"Catch event Webcam_SharpnessChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_Esi_WALLockCountdownChanged(object sender, Esi_WALLockCountdownChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_Esi_WALLockCountdownChanged",
                                    e.DeviceId, $"NewValue:{e.WALLockCountdown}"));

            writelog($"Catch event _Webcamcom_Esi_WALLockCountdownChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_Esi_IsWALLockCountdownStartedChanged(object sender, Esi_IsWALLockCountdownStartedChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_Esi_IsWALLockCountdownStartedChanged",
                                    e.DeviceId, $"NewValue:{e.IsWALLockCountdownStarted}"));

            writelog($"Catch event _Webcamcom_Esi_IsWALLockCountdownStartedChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_WALSnoozeTimeLeftInSecondsChanged(object sender, WALSnoozeTimeLeftInSecondsChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_WALSnoozeTimeLeftInSecondsChanged",
                                    e.DeviceId, $"NewValue:{e.WALSnoozeTimeLeftInSeconds}"));

            writelog($"Catch event _Webcamcom_WALSnoozeTimeLeftInSecondsChanged NewValue:{e.WALSnoozeTimeLeftInSeconds}");
            writelog($"Catch event _Webcamcom_WALSnoozeTimeLeftInSecondsChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsZoomScreenShareActiveChanged(object sender, IsZoomScreenShareActiveChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsZoomScreenShareActiveChanged",
                                    e.DeviceId, $"NewValue:{e.IsZoomScreenShareActive}"));

            writelog($"Catch event _Webcamcom_IsZoomScreenShareActiveChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsZoomMeetingActiveChanged(object sender, IsZoomMeetingActiveChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsZoomMeetingActiveChanged",
                                    e.DeviceId, $"NewValue:{e.IsZoomMeetingActive}"));

            writelog($"Catch event _Webcamcom_IsZoomMeetingActiveChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_SerialNumberChanged(object sender, SerialNumberChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_SerialNumberChanged",
                                    e.DeviceId, $"NewValue:{e.SerialNumber}"));

            writelog($"Catch event _Webcamcom_SerialNumberChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsHDROnChanged(object sender, IsHDROnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsHDROnChanged",
                                    e.DeviceId, $"NewValue:{e.IsHDROn}"));

            writelog($"Catch event _Webcamcom_IsHDROnChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_FieldOfViewChanged(object sender, FieldOfViewChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_FieldOfViewChanged",
                                    e.DeviceId, $"NewValue:{e.FieldOfView}"));

            writelog($"Catch event _Webcamcom_FieldOfViewChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_AutoFramingFrameSizeChanged(object sender, AutoFramingFrameSizeChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_AutoFramingFrameSizeChanged",
                                    e.DeviceId, $"NewValue:{e.AutoFramingFrameSize}"));

            writelog($"Catch event _Webcamcom_AutoFramingFrameSizeChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_AutoFramingSensitivityChanged(object sender, AutoFramingSensitivityChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_AutoFramingSensitivityChanged",
                                    e.DeviceId, $"NewValue:{e.AutoFramingSensitivity}"));

            writelog($"Catch event _Webcamcom_AutoFramingSensitivityChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsAutoFramingOnChanged(object sender, IsAutoFramingOnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsAutoFramingOnChanged",
                                    e.DeviceId, $"NewValue:{e.IsAutoFramingOn}"));

            writelog($"Catch event _Webcamcom_IsAutoFramingOnChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsAutoFramingTransitionOnChanged(object sender, IsAutoFramingTransitionOnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsAutoFramingTransitionOnChanged",
                                    e.DeviceId, $"NewValue:{e.IsAutoFramingTransitionOn}"));

            writelog($"Catch event _Webcamcom_IsAutoFramingTransitionOnChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_AutoWhiteBalanceChanged(object sender, AutoWhiteBalanceChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_AutoWhiteBalanceChanged",
                                    e.DeviceId, $"NewValue:{e.AutoWhiteBalance}"));

            writelog($"Catch event _Webcamcom_AutoWhiteBalanceChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsAutoWhiteBalanceOnChanged(object sender, IsAutoWhiteBalanceOnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsAutoWhiteBalanceOnChanged",
                                    e.DeviceId, $"NewValue:{e.IsAutoWhiteBalanceOn}"));

            writelog($"Catch event _Webcamcom_IsAutoWhiteBalanceOnChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_SaturationChanged(object sender, SaturationChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_SaturationChanged",
                                    e.DeviceId, $"NewValue:{e.Saturation}"));

            writelog($"Catch event _Webcamcom_SaturationChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_AntiFlickerChanged(object sender, AntiFlickerChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_AntiFlickerChanged",
                                    e.DeviceId, $"NewValue:{e.AntiFlicker}"));

            writelog($"Catch event _Webcamcom_AntiFlickerChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_ContrastChanged(object sender, ContrastChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_ContrastChanged",
                                    e.DeviceId, $"NewValue:{e.Contrast}"));

            writelog($"Catch event _Webcamcom_ContrastChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_BrightnessChanged(object sender, BrightnessChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_BrightnessChanged",
                                    e.DeviceId, $"NewValue:{e.Brightness}"));

            writelog($"Catch event _Webcamcom_BrightnessChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_ZoomChanged(object sender, ZoomChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_ZoomChanged",
                                    e.DeviceId, $"NewValue:{e.Zoom}"));

            writelog($"Catch event _Webcamcom_ZoomChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_TiltChanged(object sender, TiltChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_TiltChanged",
                                    e.DeviceId, $"NewValue:{e.Tilt}"));

            writelog($"Catch event _Webcamcom_TiltChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_PanChanged(object sender, PanChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_PanChanged",
                                    e.DeviceId, $"NewValue:{e.Pan}"));

            writelog($"Catch event _Webcamcom_PanChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_FocusChanged(object sender, FocusChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_FocusChanged",
                                    e.DeviceId, $"NewValue:{e.Focus}"));

            writelog($"Catch event _Webcamcom_FocusChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsFocusOnChanged(object sender, IsFocusOnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsFocusOnChanged",
                                    e.DeviceId, $"NewValue:{e.IsFocusOn}"));

            writelog($"Catch event _Webcamcom_IsFocusOnChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_PriorityChanged(object sender, PriorityChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_PriorityChanged",
                                    e.DeviceId, $"NewValue:{e.Priority}"));

            writelog($"Catch event _Webcamcom_PriorityChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_CustomProfileRemoved(object sender, CustomProfileRemovedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_CustomProfileRemoved",
                                    e.DeviceId, $"NewValue:{e.ProfileId}"));

            writelog($"Catch event _Webcamcom_CustomProfileRemoved : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_CustomProfileAdded(object sender, CustomProfileAddedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_CustomProfileAdded",
                                    e.DeviceId, $"NewValue:{e.ProfileId}"));

            writelog($"Catch event _Webcamcom_CustomProfileAdded : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_CurrentSelectedProfileChanged(object sender, CurrentSelectedProfileChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_CurrentSelectedProfileChanged",
                                    e.DeviceId, $"NewValue:{e.ProfileId}"));

            writelog($"Catch event _Webcamcom_CurrentSelectedProfileChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_IsMicEnumerationOnChanged(object sender, IsMicEnumerationOnChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_IsMicEnumerationOnChanged",
                                    e.DeviceId, $"NewValue:{e.IsMicEnumerationOn}"));

            writelog($"Catch event _Webcamcom_IsMicEnumerationOnChanged : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_ProfileManagerAdded(object sender, ProfileManagerAddedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_ProfileManagerAdded",
                                    e.DeviceId, $"NewValue:{e.ProfileMangerId}"));

            writelog($"Catch event _Webcamcom_ProfileManagerAdded : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_Esi_IsCameraSensorCoveredChanged(object sender, Esi_IsCameraSensorCoveredChangedArgs e)
        {
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_Esi_IsCameraSensorCoveredChanged",
                                    e.DeviceId, $"NewValue:{e.IsCameraSensorCovered}"));

            writelog($"Catch event _Webcamcom_Esi_IsCameraSensorCoveredChanged : new IsCameraSensorCovered is {e.IsCameraSensorCovered} {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_Disconnected(object sender, DisconnectedArgs e)
        {
            _ = UnregisterEventsForAllWebcamsAsync();
            //_ = UnregisterEventsForWebcamAsync();
            _ = RegisterEventsForAllWebcamsAsync();

            //SendDTPEventToUI($"3;Device:Webcam;Event:Disconnected;DeviceId:{e.DeviceId}");
            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_Disconnected", e.DeviceId));

            writelog($"Catch event _Webcam_Disconnected, current devCount is {GetWebcamDevsCountAsync().Result} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
        }

        private void Webcam_Connected(object sender, ConnectedArgs e)
        {
            Task<int> webcams = GetWebcamDevsCountAsync();
            bool result = RegisterEventsForWebcamAsync(webcams.Result - 1).Result;

            SendDTPEventToUI(CreateEventMsg("Webcam", "Webcam_Connected", e.DeviceId));

            writelog($"Catch event _Webcam_Connected, register evnet result is {result} : {DateTime.Now.ToString("hh.mm.ss.ffffff")}");
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
    }
}