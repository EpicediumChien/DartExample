#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.UX.WPF;

namespace NGA.ThickClient.Interfaces
{
    /// <summary>
    /// Provides details about the corresponding systray to the thick client
    /// When asking for this interface via plugin constructor you must assume the value could be null
    /// </summary>
    public interface ISystrayDetails
    {
        /// <summary>
        /// Unique <see cref="IConsoleConfig.ConsoleUniqueGuid"/> for the systray
        /// </summary>
        Guid SystrayUniqueId { get; }

        /// <summary>
        /// The path to systray executable
        /// </summary>
        string SystrayFullPath { get; }
    }
}