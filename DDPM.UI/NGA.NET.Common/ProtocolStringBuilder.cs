#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using NGA.NET.Common.Helpers;
using NGA.NET.Common.Interfaces;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace NGA.NET.Common
{
    /// <summary>
    /// ProtocolStringBuilder builds string version the Protocol URI
    /// </summary>
#pragma warning disable 0612, 0618
    public sealed class ProtocolStringBuilder : IProtocolStringBuilder
#pragma warning disable 0612, 0618
    {
        #region Variables

        private ShowPluginCommand _startingPluginCommand;

        #endregion

        #region IProtocolStringBuilder

        /// <summary>
        /// Adds the Starting plugin by creating plugin command.
        /// </summary>
        /// <param name="startingPlugin">Starting plugin</param>
        /// <returns>IProtocolStringBuilder</returns>
        public IProtocolStringBuilder AddStartingPlugin(Guid startingPlugin)
        {
            return AddStartingPlugin(startingPlugin, null, null);
        }

        /// <summary>
        /// Adds the Starting plugin with parameter by creating plugin command.
        /// </summary>
        /// <param name="startingPlugin">Starting plugin</param>
        /// <param name="pluginParameter">Parameter data to pass to the plugin</param>
        /// <returns>IProtocolStringBuilder</returns>
        public IProtocolStringBuilder AddStartingPlugin(Guid startingPlugin, string pluginParameter)
        {
            return AddStartingPlugin(startingPlugin, pluginParameter, null);
        }

        /// <summary>
        /// Adds the Starting plugin with parameter and priority by creating plugin command.
        /// </summary>
        /// <param name="startingPlugin">Starting plugin</param>
        /// <param name="pluginParameter">Parameter data to pass to the plugin</param>
        /// <param name="pluginPriority">Priority to display the starting plugin</param>
        /// <returns>IProtocolStringBuilder</returns>
        public IProtocolStringBuilder AddStartingPlugin(Guid startingPlugin, string pluginParameter, int? pluginPriority)
        {
            if (startingPlugin == Guid.Empty)
                throw new ArgumentException("StartingPlugin cannot be empty id", nameof(startingPlugin));

            _startingPluginCommand = new ShowPluginCommand(startingPlugin, pluginParameter, pluginPriority);
            return this;
        }

        /// <summary>
        /// Builds Launch String for protocol URI
        /// </summary>
        /// <returns>Base64Encode string</returns>
        public string BuildLaunchString()
        {
            string result = string.Empty;

            if (_startingPluginCommand != null && _startingPluginCommand.PluginId != Guid.Empty)
            {
                JsonSerializerOptions jso = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                var paramModel = new ParamModel(CommandType.ShowPlugin, Versions.ShowPluginCommandVersion2, JsonSerializer.Serialize(_startingPluginCommand, jso));
                var root = new Root(Versions.ModelVersion1, JsonSerializer.Serialize(paramModel, jso));
                result = Base64Helper.Base64Encode(JsonSerializer.Serialize(root, jso));
            }

            return $"{Versions.ApplicationUri}:{result}";
        }

        #endregion
    }
}
