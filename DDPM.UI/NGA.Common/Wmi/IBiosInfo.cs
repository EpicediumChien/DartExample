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
    public interface IBiosInfo
    {
        /// <summary>
        ///     Name of the BIOS - This is an implementation defined value. It could be equivalent to SMBiosBiosVersion, Manufacturer or some other string.
        /// </summary>      
        string Name { get; set; }

        /// <summary>
        ///     The manufacturer of the BIOS - e.g. "Dell Inc."
        /// </summary>         
        string Manufacturer { get; set; }

        /// <summary>
        ///     The serial number of the BIOS - e.g. "B7F3BK1".
        /// </summary>         
        string SerialNumber { get; set; }

        /// <summary>
        ///     The version of the BIOS - e.g. "DELL - 27da0813"
        /// </summary>         
        string Version { get; set; }

        /// <summary>
        ///     The version of the SMBIOS spec being used - e.g. "2.4" or "3.2" (see the SMBIOS Specification)
        /// </summary>         
        string SMBiosVersion { get; set; }

        /// <summary>
        ///     The BIOS version as reported by SMBIOS - e.g. "1.2.1"
        /// </summary>         
        string SMBiosBiosVersion { get; set; }
    }
}
