#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// PeripheralPlugin.cs created on 30/4/2022T3:37 PM
//

#endregion

using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using DDPM.SA.Common;
using System.Linq;
using Dell.Client.Framework.Common.Annotations;
using System.Threading;
using Dell.TechHub.Sdk.Exceptions;
using Dell.TechHub.Commodity;
using Dell.TechHub.Sdk.Common.Identifiers;
using Dell.TechHub.Common;
using Dell.TechHub.Common.Attributes;
using Dell.TechHub.Versioning;
using System.Collections;

namespace DDPM.SA.Plugins.User.DTPProxy {
  [Plugin(IDs.DDPM_DTP_Proxy_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
  [PluginRequires(Id = "{743D1C20-7A3E-4562-8D3E-C58F6ADFC050}", Version = "1.0.0", AllowDynamicResolving = true)]
  [Descriptor(Description = pluginDescription)]
  [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
  [PublishedUnelevatedInterface(new[] { typeof(IDTPProxyPlugin) })]
  public class DTPProxyPlugin : BaseAgentPlugin, IDTPProxyPlugin {
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

    private Type _mouseInterface;

    public const string PluginLogId = "DTPProxy";


    #endregion

    #region Private Members


    #endregion


    #region Constructor

    public DTPProxyPlugin(IAgent agent) : base(agent, PluginLogId) {
      _agent = agent;

      writelog("DTPProxyPlugin constructor ...");
      writelog($"Initializing the Commodity Client SDK...");
      InitializeDTPProxy();
      writelog($"Initialized successfully");
    }

    #endregion


    public event EventHandler<DeviceChangedEventArgs> Notify;

    public event EventHandler<bool> UpdateNotify;

    public void NotifyNow() {
      OnNotify(new DeviceChangedEventArgs());
    }

    public Task<int> GetDpiValue(ItemId item) {
      //_mouseInterface = FindCommodityInterfaceType("IMouseCommodity");
      //var methodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
      //                                            .MakeGenericMethod(commodityInterfaceType);
      //var value = _mouseInterface.GetProperty("DpiValue").GetGetMethod().Invoke(commodity, null);
      //writelog($"GetDpiValue called - Item \"{commodity.ItemId}\": Value: {value}");
      //return int.Parse(value.ToString());
      return Task.Run(() => 1000);
    }

    public void SetDPIValue(int newDPIValue, Guid deviceId) {
    }

    public void SetPrimaryMouseButton(string newMouseButton, Guid deviceId) {
    }


    #region Overriding methods

    protected override void OnPluginStarting() {
      _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

      PluginCondition = new PluginStartedCondition();
      writelog("DTPProxyPlugin plugin started");
    }

    #endregion
    private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e) {
      if(e?.ChangedPlugins == null || !e.ChangedPlugins.Any()) {
      }
      if(e.ChangedPlugins.OfType<ICommodityClientSdk>().Any())
        InitializeDTPProxy();
    }


    #region EventHandlers

    private void OnNotify(DeviceChangedEventArgs e) {
      if(Notify != null)
        Notify(this, e);
    }

    #endregion
    /// <summary>
    /// //
    /// </summary>
    /// <param name="text"></param>
    /// <param name="log_type">0 means info, others means error</param>
    private void writelog(string text, log_type log_type = log_type.info) {
      text = "[DeviceManager] " + text;
      Console.WriteLine(text);

      if(Log != null) {
        if(log_type == log_type.info)
          Log.Info(text);
        else
          Log.Error(text);
      }
    }
    private enum log_type {
      info = 0,
      error
    }

    Type FindCommodityInterfaceType(string commodityName) {
      foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => IsAssemblyCanditate(a))) {
        try {
          var type = assembly.GetExportedTypes()
                             .FirstOrDefault(t => t.FullName.Equals($"Dell.TechHub.Commodity.Peripheral.{commodityName}", StringComparison.OrdinalIgnoreCase));
          if(type is not null)
            return type;
        }
        catch(Exception ex) {
          writelog($"Failed to get exported type from assembly {assembly.FullName}:{ex}");
        }
      }
      return null;
    }

    ICommodity GetCommodityInterfaceInstance(MethodInfo methodInfo, ItemId itemId) {
      dynamic rawResult = methodInfo.Invoke(_commSdk, new object[] { itemId, new CancellationTokenSource().Token });
      return rawResult is null ? null : (ICommodity)rawResult;
    }

    bool IsAssemblyCanditate(Assembly a)
        => !a.IsDynamic
        && a.FullName is var fullName
        && IsCanditate(fullName, "Dell.")
        && !IsCanditate(fullName, "Dell.TechHub.Sdk.")
        && !IsCanditate(fullName, "Dell.UnifiedAgent.")
        && !IsCanditate(fullName, "Dell.Client.")
        && !IsCanditate(fullName, "Dell.RPC.");

    bool IsCanditate(string name, string startsWith)
        => name.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase);

    private void InitializeDTPProxy() {
      if(_commSdk != null)
        return;

      _commSdk = (ICommodityClientSdk)_agent.PluginManager.FindPluginByType(typeof(ICommodityClientSdk));

      if(_commSdk != null) {
        Task.Run(async () =>
        {
          await _commSdk.InitializeAsync(appId, new CancellationTokenSource().Token);
          var commodityInterfaceType = FindCommodityInterfaceType("IMouseCommodity");
          var methodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                      .MakeGenericMethod(commodityInterfaceType);
          ItemId itemId = new ItemId("DellPeripheral.Mouse.0");
          string property = "ModelNumber";

          await CallForEachPropertyAsync((commodityInterfaceType, itemId, commodity, property) =>
          {
            var value = commodityInterfaceType.GetProperty(property).GetGetMethod().Invoke(commodity, null);
            Console.WriteLine($"Item \"{commodity.ItemId}\": {commodityInterfaceType}.{property} is {ToString(value)}");

            string ToString(object v) {
              if(v is null) {
                return "null";
              }
              if((v.GetType().IsGenericType && (v.GetType().GetGenericTypeDefinition() == typeof(List<>)))) {
                return $"[{string.Join(", ", ((IList)v).Cast<object>())}]";
              }
              // TODO: Might need to handle more complex types like Dictionary if needed.
              return v.ToString();
            }
          });
        });
      }
      else {
        if(_commSdk is IFrameworkPluginConditionNotification pluginCondition) {
          pluginCondition.PluginConditionChangeHandler += OnDisplayManagerPluginConditionChangeHandler;
          GetCurrentDTPProxyPluginCondition();
        }
      }
    }

    private void OnDisplayManagerPluginConditionChangeHandler(object sender, EventArgs e) {
      GetCurrentDTPProxyPluginCondition();
    }

    private void GetCurrentDTPProxyPluginCondition() {
      _ = Task.Run(async () =>
      {
        var pluginCondition = await (_commSdk as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

        lock(_PluginConditionLock) {
          if(pluginCondition is PluginErrorCondition) {
            //writelog($"{nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in an error condition");
            //_DisplayManagerPluginCondition = pluginCondition;
          }
          else if(pluginCondition is PluginRunningCondition) {
            //_DisplayManagerPluginCondition = pluginCondition;
          }
          else if(pluginCondition is PluginStartedCondition) {
            //_DisplayManagerPluginCondition = pluginCondition;
          }
        }
      });
    }

    private async Task CallForEachPropertyAsync(Action<Type /*CommodityInterfaceType*/, ItemId, ICommodity, string /*PropertyName*/> action) {
      var commodityInterfaceType = FindCommodityInterfaceType("IMouseCommodity");
      var methodInfo = typeof(ICommodityClientSdk).GetMethod("GetCommodityAsync", new[] { typeof(ItemId), typeof(CancellationToken) })
                                                  .MakeGenericMethod(commodityInterfaceType);
      ItemId itemId = new ItemId("DellPeripheral.Mouse.0");
      string property = "ModelNumber";

      try {
        if(await GetCommodityInterfaceInstanceAsync(methodInfo, itemId) is ICommodity commodity) {
          try {
            action(commodityInterfaceType, itemId, commodity, property);
          }
          catch(Exception ex) {
            Console.WriteLine($"\nError while handling {commodityInterfaceType}.{property} on item \"{itemId}\".\n{ex}");
          }
        }
        else {
          Console.WriteLine($"Could not retrieve the Commodity Interface {commodityInterfaceType} for the {itemId} item.");
        }
      }
      catch(Exception ex) {
        Console.WriteLine($"\nError handling {commodityInterfaceType}'s {itemId} item.\n{ex}");
      }
      return;

      Type FindCommodityInterfaceType(string commodityName) {
        foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => IsAssemblyCanditate(a))) {
          try {
            var type = assembly.GetExportedTypes()
                               .FirstOrDefault(t => t.FullName.Equals($"Dell.TechHub.Commodity.Peripheral.{commodityName}", StringComparison.OrdinalIgnoreCase));
            if(type is not null)
              return type;
          }
          catch(Exception ex) {
            Console.WriteLine($"\nFailed to get exported type from assembly {assembly.FullName}\n{ex}");
            Log.Trace(ex, $"Failed to get exported type from assembly {assembly.FullName}");
          }
        }
        throw new DtpException($"The {commodityName} type cannot be resolved");

        //Type Find(string commodityName) {
        //  foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()
        //                                    .Where(a => IsAssemblyCanditate(a))) {
        //    try {
        //      var type = assembly.GetExportedTypes()
        //                         .FirstOrDefault(t => t.FullName.Equals(commodityName, StringComparison.OrdinalIgnoreCase));
        //      if(type is not null)
        //        return type;
        //    }
        //    catch(Exception ex) {
        //      Log.Trace(ex, $"Failed to get exported type from assembly {assembly.FullName}");
        //    }
        //  }
        //  return null;
        //}
      }

      async Task<ICommodity> GetCommodityInterfaceInstanceAsync(MethodInfo methodInfo, ItemId itemId) {
        Console.WriteLine($"methodInfo: {methodInfo}");
        dynamic rawResult = methodInfo.Invoke(_commSdk, new object[] { itemId, new CancellationTokenSource().Token });
        Console.WriteLine($"rawResult: {rawResult}");
        return rawResult is null ? null : (ICommodity)await rawResult;
      }

      static bool IsAssemblyCanditate(Assembly a)
          => !a.IsDynamic
          && a.FullName is var fullName
          && IsCanditate(fullName, "Dell.")
          && !IsCanditate(fullName, "Dell.TechHub.Sdk.")
          && !IsCanditate(fullName, "Dell.UnifiedAgent.")
          && !IsCanditate(fullName, "Dell.Client.")
          && !IsCanditate(fullName, "Dell.RPC.");

      static bool IsCanditate(string name, string startsWith)
          => name.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase);
    }
  }
}