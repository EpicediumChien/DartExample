#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.Common;
using System.Management;

namespace NGA.Common.Wmi
{
    /// <summary>
    /// Provides service to get the WmiBiosInfo object for Windows computer.
    /// </summary>
    public class WmiBiosInfoWindows : IWmiBiosInfoService
    {
        /// <summary>
        ///     Creates a new BiosInfo object for the local computer (".")
        /// </summary>
        /// <returns></returns>
        public IBiosInfo? Discover()
        {
            return Discover(".");
        }

        /// <summary>
        ///     Creates a new BiosInfo object for the specified computer name.
        /// </summary>
        /// <param name="computerName">Computer name or "." for the local computer.</param>
        /// <returns></returns>
        public IBiosInfo? Discover(string computerName)
        {
            using var wmi = new WmiHelper("root\\cimv2", "Win32_BIOS", computerName);
            if (wmi.ObjectCount == 0)
                return null;

            var m = wmi.Collection.OfType<ManagementObject>().First();
            var info = new BiosInfo
            {
                Name = WmiHelper.GetProp(m, "Name").Trim(),
                Manufacturer = WmiHelper.GetProp(m, "Manufacturer"),
                SerialNumber = WmiHelper.GetProp(m, "SerialNumber"),
                Version = WmiHelper.GetProp(m, "Version"),
                SMBiosBiosVersion = WmiHelper.GetProp(m, "SMBiosBiosVersion")
            };

            var smBiosVersionMajor = WmiHelper.GetUInt16(m, "SMBIOSMajorVersion");
            var smBiosVersionMinor = WmiHelper.GetUInt16(m, "SMBIOSMinorVersion");
            info.SMBiosVersion = $"{smBiosVersionMajor}.{smBiosVersionMinor}";

            return info;
        }
    }
}