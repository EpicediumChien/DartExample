#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System.Linq;
using System.Management;
using Dell.Client.Framework.Common;

namespace NGA.Common.Wmi
{
    /// <summary>
    /// Provides service to get the WmiSystemInfo object for Windows computer.
    /// </summary>
    public class WmiSystemInfoWindows : IWmiSystemInfoService
    {
        /// <summary>
        ///     Creates a new SystemInfo object for the local computer (".")
        /// </summary>
        /// <returns></returns>
        public ISystemInfo? GetSystemInfo()
        {
            return GetSystemInfo(".");
        }

        /// <summary>
        ///     Creates a new SystemInfo object for the specified computer name.
        /// </summary>
        /// <param name="computerName">Computer name or "." for the local computer.</param>
        /// <returns></returns>
        public ISystemInfo? GetSystemInfo(string computerName)
        {
            using var wmi = new WmiHelper("root\\cimv2", "Win32_ComputerSystem", computerName);
            if (wmi.ObjectCount == 0) 
                return null;

            var m = wmi.Collection.OfType<ManagementObject>().First();
            var info = new SystemInfo
            {
                SystemFamily = WmiHelper.GetProp(m, "SystemFamily").Trim(),
            };

            return info;
        }
    }
}
