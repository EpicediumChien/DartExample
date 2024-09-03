#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.Common.Wmi
{
    /// <summary>
    /// Defines the interface for the <see cref="WmiBiosInfoWindows"/> object.
    /// </summary>
    public interface IWmiBiosInfoService
    {
        /// <summary>
        ///     Creates a new Wmi BiosInfo object for the local computer (".").
        /// </summary>
        /// <returns></returns>
        IBiosInfo? Discover();

        /// <summary>
        ///     Creates a new Wmi BiosInfo object for the specified computer name.
        /// </summary>
        /// <param name="computerName">Computer name or "." for the local computer.</param>
        /// <returns></returns>
        IBiosInfo? Discover(string computerName);
    }
}