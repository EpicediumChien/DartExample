#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.NET.Common
{
    /// <summary>
    /// ShowPluginCommand class
    /// </summary>
    public class ShowPluginCommand
    {
        #region Properties

        /// <summary>
        /// PluginId to Show
        /// </summary>
        public Guid PluginId { get; }

        /// <summary>
        /// Parameter data to pass to the plugin
        /// </summary>
        public string PluginParameter { get; }

        /// <summary>
        /// Plugin priority
        /// </summary>
        public int? PluginPriority { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="pluginParameter"></param>
        /// <param name="pluginPriority"></param>
        public ShowPluginCommand(Guid pluginId, string pluginParameter = null, int? pluginPriority = null)
        {
            PluginId = pluginId;
            PluginParameter = pluginParameter;
            PluginPriority = pluginPriority;
        }

        #endregion
    }
}