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

namespace DDPM.SA.Plugins.User.DTPProxy
{
    [Plugin(IDs.DDPM_DTP_Proxy_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
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

        public const string PluginLogId = "DTPProxy";

        #endregion

        #region Constructor

        public DTPProxyPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;

            writelog("DTPProxyPlugin constructor ...");
            writelog($"Initializing the Commodity Client SDK...");
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
                    Console.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
                    return -1;
                }
            }
            else
            {
                Console.WriteLine($"[GetDpiValue]Could not retrieve the Commodity Interface for the {_itemID} item. _mouseMethodInfo is null");
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
                    Console.WriteLine($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
                    writelog($"Could not retrieve the Commodity Interface {_mouseInterfaceType} for the {_itemID} item.");
                }
            }
            else
            {
                Console.WriteLine($"[SetDPIValue]Could not retrieve the Commodity Interface for the  {_itemID}  item. _mouseMethodInfo is null");
                writelog($"[SetDPIValue]Could not retrieve the Commodity Interface for the  {_itemID}  item. _mouseMethodInfo is null");
            }

        }

        public async Task SetEraserDoublePressSetting(string itemID, byte[] newValue)
        {
        }

        public async Task SetEraserLongPressSetting(string itemID, byte[] newValue)
        {
        }

        public async Task SetEraserSinglePressSetting(string itemID, byte[] newValue)
        {
        }

        public async Task SetIsSideBottomButtonHoverClick(string itemID, bool newValue)
        {
        }

        public async Task SetIsSideTopButtonHoverClick(string itemID, bool newValue)
        {
        }

        public async Task SetMenuSinglePressSetting(string itemID, byte[] newValue)
        {
        }

        public async Task SetMenuCenterRightClickSetting(string itemID, byte[] newValue)
        {
        }

        public async Task SetSideBottomSwitchSinglePressSetting(string itemID, byte[] newValue)
        {
        }

        public async Task SetSideTopSwitchSinglePressSetting(string itemID, byte[] newValue)
        {
        }

        public async Task SetTiltSensitivity(string itemID, int newValue)
        {
            _itemID = new ItemId(itemID);

            if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_penInterfaceType, commodity, "TiltSensitivity", newValue);
            }
            else
            {
                Console.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
            }
        }

        public async Task SetTipSensitivity(string itemID, int newValue)
        {
            _itemID = new ItemId(itemID);

            if (await GetCommodityInterfaceInstanceAsync(_penMethodInfo) is ICommodity commodity)
            {
                SetPropertyValue(_penInterfaceType, commodity, "TipSensitivity", newValue);
            }
            else
            {
                Console.WriteLine($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
                writelog($"Could not retrieve the Commodity Interface {_penInterfaceType} for the {_itemID} item.");
            }
        }

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
            text = "[DeviceManager] " + text;
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
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => IsAssemblyCanditate(a)))
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
                Console.WriteLine($"rawResult: {rawResult}");
                return rawResult is null ? null : (ICommodity)await rawResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError handling {_mouseInterfaceType}'s {_itemID} item.\n{ex}");
                writelog($"\nError handling {_mouseInterfaceType}'s {_itemID} item.\n{ex}");
                return null;
            }
        }

        private bool IsAssemblyCanditate(Assembly a)
            => !a.IsDynamic
            && a.FullName is var fullName
            && IsCanditate(fullName, "Dell.")
            && !IsCanditate(fullName, "Dell.TechHub.Sdk.")
            && !IsCanditate(fullName, "Dell.UnifiedAgent.")
            && !IsCanditate(fullName, "Dell.Client.")
            && !IsCanditate(fullName, "Dell.RPC.");

        private bool IsCanditate(string name, string startsWith)
            => name.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase);

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
                    _mouseInterfaceType = FindCommodityInterfaceType("IMouseCommodity");

                    if (_mouseInterfaceType != null)
                    {
                        _mouseMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                      .MakeGenericMethod(_mouseInterfaceType);
                    }

                    _webcamInterfaceType = FindCommodityInterfaceType("IWebcamCommodity");

                    if (_webcamInterfaceType != null)
                    {
                        _webcamMethodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                        .MakeGenericMethod(_webcamInterfaceType);
                    }

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
                return interfaceType.GetProperty(property).GetGetMethod().Invoke(commodity, null);
            }
            catch (Exception ex)
            {
                writelog($"Error while Getting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
                return null;
            }
        }

        private void SetPropertyValue(Type interfaceType, ICommodity commodity, string property, object value)
        {
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
            try
            {
                interfaceType.GetProperty(property).GetSetMethod().Invoke(commodity, new[] { value });
            }
            catch (Exception ex)
            {
                writelog($"Error while setting {interfaceType}.{property} on item \"{_itemID}\".\n{ex}");
            }
        }
    }
}