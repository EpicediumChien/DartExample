#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Microsoft;

namespace NGA.NET.Common
{
    /// <summary>
    /// ParamModel class
    /// </summary>
    public class ParamModel
    {
        #region Properties

        /// <summary>
        /// CommandType enum
        /// </summary>
        public CommandType CommandType { get; }

        /// <summary>
        /// Version of command
        /// </summary>
        public int CommandVersion { get; }

        /// <summary>
        /// Json serialized Command (i.e. and object of ShowPluginCommand)
        /// </summary>
        public string CommandJson { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandVersion"></param>
        /// <param name="commandJson"></param>
        public ParamModel(CommandType commandType, int commandVersion, string commandJson)
        {
            Requires.NotDefault(commandType, nameof(commandType));

            CommandType = commandType;
            CommandVersion = commandVersion;
            CommandJson = commandJson;
        }

        #endregion
    }
}
