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
    /// Defines the interface for the BiosInfo object.
    /// </summary>
    public interface ISystemInfo
    {
        /// <summary>
        /// The family to which a particular computer belongs.
        /// </summary>
        string SystemFamily { get; set; }
    }
}