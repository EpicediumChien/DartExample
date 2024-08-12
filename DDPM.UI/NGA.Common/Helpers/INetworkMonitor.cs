#region LicenseHeader
//
// ©Copyright 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;

namespace NGA.Common.Helpers
{
    /// <summary>
    /// Network Monitor interface
    /// </summary>
    public interface INetworkMonitor
    {
        /// <summary>
        /// Event that occurs after network address changes where the network is or becomes available
        /// </summary>
        event EventHandler<NetworkMonitorEventArgs> NetworkAvailableAndChanged;

        /// <summary>
        /// Get Internet Connectivity Status
        /// </summary>
        /// <returns></returns>
        bool IsInternetConnected();
    }

    /// <summary>
    /// Network Monitor EventArgs
    /// </summary>
    public class NetworkMonitorEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isInternetConnected"></param>
        public NetworkMonitorEventArgs(bool isInternetConnected)
        {
            IsInternetConnected = isInternetConnected;
        }

        /// <summary>
        /// IsInternetConnected property
        /// </summary>
        public bool IsInternetConnected  { get; }
    }
}
