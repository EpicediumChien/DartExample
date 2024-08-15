#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.NET.Common.Interfaces
{
    /// <summary>
    /// IProtocolStringBuilder builds string version the Protocol URI
    /// </summary>
    [Obsolete("Interface is deprecated, Please use INotificationProtocolURI interface to Build/Get Protocol URIs")]
    public interface IProtocolStringBuilder
    {
        /// <summary>
        /// Creates an instance of ShowPluginCommand using the Guid argument
        /// </summary>
        /// <param name="startingPlugin">Starting plugin</param>
        /// <returns>IProtocolStringBuilder</returns>
        [Obsolete("This method is deprecated, Please use INotificationProtocolURI interface to Build/Get Protocol URIs")]
        IProtocolStringBuilder AddStartingPlugin(Guid startingPlugin);

        /// <summary>
        /// Adds the Starting plugin with parameter by creating plugin command.
        /// </summary>
        /// <param name="startingPlugin">Starting plugin</param>
        /// <param name="pluginParameter">Parameter data to pass to the plugin</param>
        /// <returns>IProtocolStringBuilder</returns>
        [Obsolete("This method is deprecated, Please use INotificationProtocolURI interface to Build/Get Protocol URIs")]
        IProtocolStringBuilder AddStartingPlugin(Guid startingPlugin, string pluginParameter);

        /// <summary>
        /// Adds the Starting plugin with parameter and priority by creating plugin command.
        /// </summary>
        /// <param name="startingPlugin">Starting plugin</param>
        /// <param name="pluginParameter">Parameter data to pass to the plugin</param>
        /// <param name="pluginPriority">Priority to display the starting plugin</param>
        /// <returns>IProtocolStringBuilder</returns>
        [Obsolete("This method is deprecated, Please use INotificationProtocolURI interface to Build/Get Protocol URIs")]
        IProtocolStringBuilder AddStartingPlugin(Guid startingPlugin, string pluginParameter, int? pluginPriority);

        /// <summary>
        /// Builds Launch String for protocol URI
        /// </summary>
        /// <returns>Base64Encode string</returns>
        [Obsolete("This method is deprecated, Please use INotificationProtocolURI interface to Build/Get Protocol URIs")]
        string BuildLaunchString();
    }
}