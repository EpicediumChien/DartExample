#region LicenceHeader
//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Dell.Client.Framework.UX.WPF;
using Microsoft;
using NGA.ThickClient.Interfaces;

namespace NGA.ThickClientCore
{
    /// <inheritdoc/>
    public class DucaSystrayDetails : ISystrayDetails
    {
        /// <summary>
        /// Constructor that lets the caller set the systray full path and the unique ID for the systray
        /// </summary>
        /// <param name="systrayFullPath">The executable path for the systray so that when the console is started, the console can launch the systray if it is not running</param>
        /// <param name="systrayUniqueId">The <see cref="IConsoleConfig.ConsoleUniqueGuid"/> for the systray application</param>
        public DucaSystrayDetails(string systrayFullPath, Guid systrayUniqueId)
        {
            Requires.NotNullOrWhiteSpace(systrayFullPath, nameof(systrayFullPath));
            Requires.NotEmpty(systrayUniqueId, nameof(systrayUniqueId));
            SystrayFullPath = systrayFullPath;
            SystrayUniqueId = systrayUniqueId;
        }
        /// <inheritdoc/>
        public Guid SystrayUniqueId { get; private set; }

        /// <inheritdoc/>
        public string SystrayFullPath { get; private set; }
    }
}
