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
    /// Provides System information for the computer.
    /// </summary>
    public class SystemInfo : ISystemInfo
    {
        #region Variables

        private static readonly object InstanceLock = new();

        private static IWmiSystemInfoService? _instance;

        #endregion

        #region ISystemInfo

        /// <summary>
        ///  The family to which a particular computer belongs.
        /// </summary>
        public string SystemFamily { get; set; }

        #endregion

        #region public methods

        /// <summary>
        /// Singleton instance of SystemInfo
        /// </summary>
        public static IWmiSystemInfoService Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    return _instance ??= new WmiSystemInfoWindows();
                }
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public SystemInfo()
        {
            SystemFamily = string.Empty;
        }

        #endregion
    }
}