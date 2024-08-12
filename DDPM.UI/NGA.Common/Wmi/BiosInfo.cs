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
    /// Provides Bios information for the computer.
    /// </summary>
    public class BiosInfo : IBiosInfo
    {
        #region Variables
        private static readonly object InstanceLock = new object();

        private static IWmiBiosInfoService? _instance;
        #endregion

        #region IBiosInfo

        /// <summary>
        ///     Name of the BIOS - This is an implementation defined value. It could be equivalent to SMBiosBiosVersion, Manufacturer or some other string.
        /// </summary>      
        public string Name { get; set; }

        /// <summary>
        ///     The manufacturer of the BIOS - e.g. "Dell Inc."
        /// </summary>         
        public string Manufacturer { get; set; }

        /// <summary>
        ///     The serial number of the BIOS - e.g. "B7F3BK1".
        /// </summary>         
        public string SerialNumber { get; set; }

        /// <summary>
        ///     The version of the BIOS - e.g. "DELL - 27da0813"
        /// </summary>         
        public string Version { get; set; }

        /// <summary>
        ///     The version of the SMBIOS spec being used - e.g. "2.4" or "3.2" (see the SMBIOS Specification)
        /// </summary>         
        public string SMBiosVersion { get; set; }

        /// <summary>
        ///     The BIOS version as reported by SMBIOS - e.g. "1.2.1"
        /// </summary>         
        public string SMBiosBiosVersion { get; set; }

        #endregion

        #region public methods

        /// <summary>
        /// Singleton instance of BiosInfo
        /// </summary>
        public static IWmiBiosInfoService Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    return _instance ??= new WmiBiosInfoWindows();
                }
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public BiosInfo()
        {
            Name = string.Empty;
            Manufacturer = string.Empty;
            SerialNumber = string.Empty;
            Version = string.Empty;
            SMBiosVersion = string.Empty;
            SMBiosBiosVersion = string.Empty;
        }

        #endregion
    }
}
