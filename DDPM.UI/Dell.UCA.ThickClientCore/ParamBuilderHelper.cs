#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.Common.DataModel;
using Dell.Client.Framework.UX.WPF;
using NGA.Common;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using static Dell.Client.Framework.UX.WPF.WindowHelper;
[assembly: InternalsVisibleTo("Dell.UCA.ThickClientCore.Tests")]
namespace NGA.ThickClientCore
{
    /// <summary>
    /// Helper class for building plugin arguments
    /// </summary>
    internal static class ParamBuilderHelper
    {
        /// <summary>
        /// Helper method for building plugin command line parameters for console window
        /// </summary>
        /// <returns></returns>
        internal static List<string> CreateConsoleWindowArguments(ShowPluginCommand? pluginCommand, ILog? log)
        {
            var paramArgs = new List<string>();

            if (pluginCommand == null)
                return paramArgs;

            var pluginId = pluginCommand.PluginId.ToString();

            if (string.IsNullOrWhiteSpace(pluginId) || pluginCommand.PluginId == Guid.Empty)
                return paramArgs;

            //Add plugin id
            paramArgs.Add($"--{Constants.StartingPlugin}={pluginId}");

            var param = pluginCommand.PluginParameter;

            if (!string.IsNullOrWhiteSpace(param))
            {
                JsonSerializerOptions jso = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

                var model = new ParameterModel(param);
                var startPluginModel = new StartingPluginParameterModel(Constants.StartingPluginParameterModelVersion1,
                    JsonSerializer.Serialize(model, jso));

                //Add plugin parameter
                paramArgs.Add($"--{Constants.StartingPluginParameter}={JsonSerializer.Serialize(startPluginModel, jso)}");
            }

            if (pluginCommand.PluginPriority != null)
            {
                //Add plugin priority
                paramArgs.Add($"--{Constants.StartingPluginPriority}={pluginCommand.PluginPriority}");
            }

            log?.Trace($"{nameof(CreateConsoleWindowArguments)}: plugin parameters:{string.Join(" ", paramArgs)}");

            return paramArgs;
        }

        /// <summary>
        /// Helper method for building plugin data objects
        /// </summary>
        /// <param name="pluginCommand"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        internal static PLUGIN_DATA? CreatePluginData(ShowPluginCommand? pluginCommand, ILog? log)
        {
            if (pluginCommand == null)
                return null;

            var dataObjects = CreateParamDataObjects(pluginCommand, log);

            if (dataObjects.Count == 0)
                return null;

            var pluginData = new PLUGIN_DATA
            {
                PluginParameterData = dataObjects.ToList()
            };
            return pluginData;
        }

        private static List<PLUGIN_PARAMETER_DATA> CreateParamDataObjects(ShowPluginCommand pluginCommand, ILog? log)
        {
            var paramArgs = new List<PLUGIN_PARAMETER_DATA>();

            var pluginId = pluginCommand.PluginId.ToString();

            if (string.IsNullOrWhiteSpace(pluginId) || pluginCommand.PluginId == Guid.Empty)
                return paramArgs;

            //Add plugin Id
            paramArgs.Add(new PLUGIN_PARAMETER_DATA()
            {
                Version = Constants.StartingPluginParameterModelVersion1.ToString(),
                Name = Constants.StartingPlugin,
                Data = pluginId
            });

            var param = pluginCommand.PluginParameter;

            if (!string.IsNullOrWhiteSpace(param))
            {
                JsonSerializerOptions jso = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

                var model = new ParameterModel(param);
                var startPluginModel = new StartingPluginParameterModel(Constants.StartingPluginParameterModelVersion1,
                    JsonSerializer.Serialize(model, jso));

                //Add plugin parameter
                paramArgs.Add(new PLUGIN_PARAMETER_DATA()
                {
                    Version = Constants.StartingPluginParameterModelVersion1.ToString(),
                    Name = Constants.StartingPluginParameter,
                    Data = JsonSerializer.Serialize(startPluginModel, jso)
                }
                );
            }

            if (pluginCommand.PluginPriority != null)
            {
                //Add plugin priority
                paramArgs.Add(new PLUGIN_PARAMETER_DATA()
                {
                    Version = Constants.StartingPluginParameterModelVersion1.ToString(),
                    Name = Constants.StartingPluginPriority,
                    Data = pluginCommand.PluginPriority.ToString()
                });
            }

            var traceLog = paramArgs.Aggregate("plugin parameters:", (current, arg) => current + $" {arg.Name}:{arg.Data}");

            log?.Trace($"{nameof(CreateParamDataObjects)}: {traceLog}");

            return paramArgs;
        }
    }
}